using System.Text;

namespace indentChecker
{
    class Program
    {
        static readonly string[] RequiredFiles = new[]
        {
            "ignoredFullPaths.txt",
            "ignoredNames.txt",
            "ignoredExtensions.txt",
            "scanRoots.txt",
            "encodingMap.json"
        };

        static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        static void Main(string[] args)
        {
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
                var ignoredFullPaths = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredFullPaths.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var ignoredNames = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredNames.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var ignoredExtensions = new HashSet<string>(new TrimmedLinesFile(GetAbsolutePath("ignoredExtensions.txt")).Lines, StringComparer.OrdinalIgnoreCase);
                var scanRoots = new TrimmedLinesFile(GetAbsolutePath("scanRoots.txt")).Lines;
                var encodingMap = new EncodingMapFile(GetAbsolutePath("encodingMap.json"));

                var filesNeedingAttention = new List<(string File, string Message)>();

                // Sort scan roots before processing
                var sortedScanRoots = scanRoots.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

                var stats = new ScanStatistics();

                foreach (var root in sortedScanRoots)
                {
                    if (!Directory.Exists(root))
                    {
                        Console.WriteLine($"Root directory not found: {root}");
                        continue;
                    }
                    ScanDirectory(root, ignoredFullPaths, ignoredNames, ignoredExtensions, encodingMap, filesNeedingAttention, stats);
                }

                if (filesNeedingAttention.Count == 0)
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

        static void ScanDirectory(string dir, HashSet<string> ignoredFullPaths, HashSet<string> ignoredNames, EncodingMapFile encodingMap, List<(string File, string Message)> filesNeedingAttention)
        {
            // Ignore this directory if in ignoredFullPaths or ignoredNames
            if (ignoredFullPaths.Contains(dir) || ignoredNames.Contains(Path.GetFileName(dir)))
                return;

            // Dive into subdirectories first, sorted
            var subdirs = Directory.GetDirectories(dir).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var subdir in subdirs)
            {
                ScanDirectory(subdir, ignoredFullPaths, ignoredNames, encodingMap, filesNeedingAttention);
            }

            // Then process files, sorted
            var files = Directory.GetFiles(dir).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (ignoredFullPaths.Contains(file) || ignoredNames.Contains(Path.GetFileName(file)))
                    continue;

                string ext = Path.GetExtension(file);
                string encodingName = encodingMap.GetEncodingName(file) ?? encodingMap.GetEncodingName(ext) ?? "utf-8";
                Encoding encoding;
                try {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (Exception ex) {
                    ConsoleColorUtil.WriteLine($"Invalid encoding '{encodingName}' for file '{file}': {ex}", ConsoleColor.Red);
                    continue;
                }

                string[] lines;
                try { lines = File.ReadAllLines(file, encoding); }
                catch (Exception ex) {
                    ConsoleColorUtil.WriteLine($"Failed to read file '{file}' with encoding '{encodingName}': {ex}", ConsoleColor.Red);
                    continue;
                }

                string message;
                if (NeedsIndentationAttention(lines, out message))
                {
                    filesNeedingAttention.Add((file, message));
                    ConsoleColorUtil.WriteLine($"{file}: {message}", ConsoleColor.Yellow);
                }
            }
        }

        static bool NeedsIndentationAttention(string[] lines, out string message)
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
                    int spaceCount = indentLength; // all must be spaces if we get here
                    if (spaceCount % 4 != 0)
                    {
                        message = $"Indentation not a multiple of 4 spaces at line {i + 1} (found {spaceCount} spaces).";
                        return true;
                    }
                }
            }
            message = string.Empty;
            return false;
        }
    }
}
