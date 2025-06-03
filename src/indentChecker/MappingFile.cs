using System.Text.Json;

namespace indentChecker
{
    /// <summary>
    /// Loads a JSON file mapping file extensions or full paths to arbitrary string values.
    /// </summary>
    public class MappingFile
    {
        /// <summary>
        /// The mapping from extension or full path to value.
        /// </summary>
        public IReadOnlyDictionary<string, string> Map { get; }

        public MappingFile(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var map = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
            if (map == null)
                throw new InvalidDataException($"Failed to parse mapping file from {filePath}");
            Map = map;
        }

        /// <summary>
        /// Gets the mapped value for a given file path or extension, or null if not found.
        /// </summary>
        public string? GetValue(string filePathOrExtension)
        {
            Map.TryGetValue(filePathOrExtension, out var value);
            return value;
        }
    }
}
