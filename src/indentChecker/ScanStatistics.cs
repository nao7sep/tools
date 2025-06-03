using System.Text;

namespace indentChecker
{
    public class ScanStatistics
    {
        public HashSet<string> ScannedDirectories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> DetectedExtensions { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<(string File, string Encoding)> CheckedFiles { get; } = new();
        public HashSet<string> IgnoredByFullPath { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> IgnoredByName { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> IgnoredByExtension { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<(string File, string Encoding, string Message)> FilesWithIssues { get; } = new();
        public List<string> Errors { get; } = new();

        public void AddScannedDirectory(string dir) => ScannedDirectories.Add(dir);
        public void AddDetectedExtension(string ext)
        {
            if (!string.IsNullOrEmpty(ext))
                DetectedExtensions.Add(ext);
        }
        public void AddCheckedFile(string file, string encoding) => CheckedFiles.Add((file, encoding));
        public void AddIgnoredByFullPath(string file) => IgnoredByFullPath.Add(file);
        public void AddIgnoredByName(string file) => IgnoredByName.Add(file);
        public void AddIgnoredByExtension(string file) => IgnoredByExtension.Add(file);
        public void AddFileWithIssue(string file, string encoding, string message) => FilesWithIssues.Add((file, encoding, message));
        public void AddError(string error) => Errors.Add(error);

        public void WriteReport(string reportFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Scan Report");
            sb.AppendLine();
            sb.AppendLine("[Scanned Directories]");
            foreach (var dir in ScannedDirectories.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(dir);
            sb.AppendLine();
            sb.AppendLine("[Detected Extensions]");
            foreach (var ext in DetectedExtensions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(ext.ToLowerInvariant());
            sb.AppendLine();
            sb.AppendLine("[Checked Files]");
            foreach (var (file, encoding) in CheckedFiles.OrderBy(x => x.File, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"{file} (encoding: {encoding})");
            sb.AppendLine();
            sb.AppendLine("[Ignored by Full Path]");
            foreach (var file in IgnoredByFullPath.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(file);
            sb.AppendLine();
            sb.AppendLine("[Ignored by Name]");
            foreach (var file in IgnoredByName.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(file);
            sb.AppendLine();
            sb.AppendLine("[Ignored by Extension]");
            foreach (var file in IgnoredByExtension.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(file);
            sb.AppendLine();
            sb.AppendLine("[Files with Issues]");
            foreach (var (file, encoding, message) in FilesWithIssues.OrderBy(x => x.File, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"{file} (encoding: {encoding}): {message}");
            if (Errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("[Errors]");
                foreach (var err in Errors)
                    sb.AppendLine(err);
            }
            File.WriteAllText(reportFilePath, sb.ToString(), new UTF8Encoding(true)); // BOM-ed UTF-8
        }
    }
}
