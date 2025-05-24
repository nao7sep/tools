namespace toSrgb
{
    /// <summary>
    /// Parses command line arguments for the toSrgb application.
    /// </summary>
    public class CommandLineOptions
    {
        public int? JpegQuality { get; set; }
        public List<string> ImagePaths { get; } = [];

        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, "--quality", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --quality parameter.");
                    }
                    string value = args[++i];
                    if (int.TryParse(value, out int q) && q >= 1 && q <= 100)
                    {
                        options.JpegQuality = q;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid JPEG quality. Must be 1-100.");
                    }
                }
                else
                {
                    options.ImagePaths.Add(arg);
                }
            }
            return options;
        }
    }
}
