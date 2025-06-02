using System.Text;

namespace indentChecker
{
    /// <summary>
    /// Loads a UTF-8 text file, splits into lines, trims each line, and stores non-empty lines.
    /// </summary>
    public class TrimmedLinesFile
    {
        /// <summary>
        /// The visible (non-empty, trimmed) lines from the file.
        /// </summary>
        public IReadOnlyList<string> Lines { get; }

        public TrimmedLinesFile(string filePath)
        {
            var lines = new List<string>();
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            using (var reader = new StringReader(content))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        lines.Add(trimmed);
                }
            }
            Lines = lines;
        }
    }
}
