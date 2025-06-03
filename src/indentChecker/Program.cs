using System.Text;

namespace indentChecker
{
    class Program
    {
        static readonly string[] RequiredFiles = new[]
        {
            "scanRoots.txt",
            "ignoredFullPaths.txt",
            "ignoredNames.txt",
            "ignoredExtensions.txt",
            "encodingMap.json",
            "indentModeMap.json"
        };

        static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        static void Main(string[] args)
        {
            // Register additional encodings (e.g., shift_jis)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                // Map required files to absolute paths
                var requiredFilePaths = RequiredFiles.Select(GetAbsolutePath).ToArray();
                var missingFiles = requiredFilePaths.Where(f => !File.Exists(f)).ToList();
                if (missingFiles.Count > 0)
                {
                    foreach (var file in missingFiles)
                        ConsoleColorUtil.WriteLine($"Required file missing: {file}", ConsoleColor.Red);
                    return;
                }

                // Load config files using absolute paths
                var scanRoots = new TrimmedLinesFile(GetAbsolutePath("scanRoots.txt")).Lines;
                var ignoredFullPaths = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredFullPaths.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var ignoredNames = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredNames.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var ignoredExtensions = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredExtensions.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var encodingMap = new MappingFile(GetAbsolutePath("encodingMap.json"));
                var indentModeMap = new MappingFile(GetAbsolutePath("indentModeMap.json"));

                var stats = new ScanStatistics();

                var sortedScanRoots = scanRoots.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

                foreach (var root in sortedScanRoots)
                {
                    if (!Directory.Exists(root))
                    {
                        Console.WriteLine($"Root directory not found: {root}");
                        continue;
                    }
                    ScanDirectory(root, ignoredFullPaths, ignoredNames, ignoredExtensions, encodingMap, indentModeMap, stats);
                }

                // Write statistics report to Desktop with UTC timestamp in the filename
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string utcNow = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
                string reportFile = Path.Combine(desktopPath, $"indentChecker-{utcNow}.log");
                stats.WriteReport(reportFile);

                if (stats.FilesWithIssues.Count == 0)
                {
                    Console.WriteLine("No indentation issues found.");
                }
            }
            catch (Exception ex)
            {
                ConsoleColorUtil.WriteLine($"Error: {ex}", ConsoleColor.Red);
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void ScanDirectory(string dir, HashSet<string> ignoredFullPaths, HashSet<string> ignoredNames, HashSet<string> ignoredExtensions, MappingFile encodingMap, MappingFile indentModeMap, ScanStatistics stats)
        {
            if (ignoredFullPaths.Contains(dir)) { stats.AddIgnoredByFullPath(dir); return; }
            if (ignoredNames.Contains(Path.GetFileName(dir))) { stats.AddIgnoredByName(dir); return; }
            stats.AddScannedDirectory(dir);

            // Dive into subdirectories first, sorted
            var subdirs = Directory.GetDirectories(dir).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var subdir in subdirs)
            {
                ScanDirectory(subdir, ignoredFullPaths, ignoredNames, ignoredExtensions, encodingMap, indentModeMap, stats);
            }

            // Then process files, sorted
            var files = Directory.GetFiles(dir).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (ignoredFullPaths.Contains(file)) { stats.AddIgnoredByFullPath(file); continue; }
                if (ignoredNames.Contains(Path.GetFileName(file))) { stats.AddIgnoredByName(file); continue; }
                if (ignoredExtensions.Contains(Path.GetExtension(file))) { stats.AddIgnoredByExtension(file); continue; }

                string ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext))
                    stats.AddDetectedExtension(ext);

                string encodingName = encodingMap.GetValue(file) ?? encodingMap.GetValue(ext) ?? "utf-8";
                Encoding encoding;
                try {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (Exception ex) {
                    string err = $"Invalid encoding '{encodingName}' for file '{file}': {ex}";
                    ConsoleColorUtil.WriteLine(err, ConsoleColor.Red);
                    stats.AddError(err);
                    continue;
                }

                string[] lines;
                try { lines = File.ReadAllLines(file, encoding); }
                catch (Exception ex) {
                    string err = $"Failed to read file '{file}' with encoding '{encodingName}': {ex}";
                    ConsoleColorUtil.WriteLine(err, ConsoleColor.Red);
                    stats.AddError(err);
                    continue;
                }

                // Determine indent mode before recording checked file
                string? indentModeStr = indentModeMap.GetValue(file) ?? indentModeMap.GetValue(ext);
                IndentMode indentMode = ParseIndentMode(indentModeStr);
                stats.AddCheckedFile(file, encodingName, indentMode);

                string message;
                if (NeedsIndentationAttention(lines, indentMode, out message))
                {
                    stats.AddFileWithIssue(file, encodingName, indentMode, message);
                    ConsoleColorUtil.WriteLine($"{file}: {message}", ConsoleColor.Yellow);
                }
            }
        }

        static bool NeedsIndentationAttention(string[] lines, IndentMode indentMode, out string message)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                int indentLength = 0;
                // Count indentation chars (all leading whitespace)
                while (indentLength < line.Length && char.IsWhiteSpace(line[indentLength]))
                {
                    indentLength++;
                }
                if (indentLength > 0)
                {
                    // If the whole line is indentation, it's an error (check this first)
                    if (indentLength == line.Length)
                    {
                        message = $"Line {i + 1} contains only indentation and no visible characters.";
                        return true;
                    }
                    // Check for any non-ASCII-space char in indentation
                    for (int j = 0; j < indentLength; j++)
                    {
                        char c = line[j];
                        if (c != ' ')
                        {
                            message = $"Non-ASCII-space char (U+{((int)c):X4}) used for indentation at line {i + 1}.";
                            return true;
                        }
                    }
                    // Check for indentation less than 4 spaces
                    if (indentLength < 4)
                    {
                        message = $"Indentation less than 4 spaces at line {i + 1} (found {indentLength} spaces).";
                        return true;
                    }
                    // If strict mode, check for indentation not divisible by 4
                    if (indentMode == IndentMode.Strict && indentLength % 4 != 0)
                    {
                        message = $"Indentation not a multiple of 4 spaces at line {i + 1} (found {indentLength} spaces).";
                        return true;
                    }
                }
            }
            message = string.Empty;
            return false;
        }

        // ParseIndentMode: strict is fallback if input is null/invalid
        static IndentMode ParseIndentMode(string? s)
        {
            if (string.Equals(s, "flex", StringComparison.OrdinalIgnoreCase)) return IndentMode.Flex;
            return IndentMode.Strict;
        }
    }
}
