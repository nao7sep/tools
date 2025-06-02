using System.Text.Json;

namespace indentChecker
{
    /// <summary>
    /// Loads a JSON file mapping file extensions or full paths to encoding names.
    /// </summary>
    public class EncodingMapFile
    {
        /// <summary>
        /// The mapping from extension or full path to encoding name.
        /// </summary>
        public IReadOnlyDictionary<string, string> Map { get; }

        public EncodingMapFile(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var map = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
            if (map == null)
                throw new InvalidDataException($"Failed to parse encoding map from {filePath}");
            Map = map;
        }

        /// <summary>
        /// Gets the encoding name for a given file path or extension, or null if not found.
        /// </summary>
        public string? GetEncodingName(string filePathOrExtension)
        {
            Map.TryGetValue(filePathOrExtension, out var encodingName);
            return encodingName;
        }
    }
}
