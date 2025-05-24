namespace toSrgb
{
    /// <summary>
    /// Handles interactive console input for selecting JPEG quality.
    /// </summary>
    public static class JpegQualitySelector
    {
        /// <summary>
        /// Prompts the user to select a JPEG quality level (compact, balanced, detailed).
        /// </summary>
        /// <returns>The selected JPEG quality (75, 85, or 95), or null if the user cancels or enters invalid input.</returns>
        public static int? PromptForQuality()
        {
            Console.WriteLine("Select JPEG quality level:");
            Console.WriteLine("c - Compact (Quality 75)   : Prioritizes smaller file size with acceptable quality.");
            Console.WriteLine("    Suitable for sharing, previews, or space-limited storage.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    Metadata will be stripped.");
            Console.ResetColor();
            Console.WriteLine("b - Balanced (Quality 85)  : A middle ground between size and quality.");
            Console.WriteLine("    Good for general use where quality matters but efficiency is still important.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    Metadata will be stripped.");
            Console.ResetColor();
            Console.WriteLine("d - Detailed (Quality 95)  : Minimizes visible compression. Ideal for editing output,");
            Console.WriteLine("    printing, or when image detail is a priority.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("    Metadata will be preserved.");
            Console.ResetColor();
            Console.Write("Enter choice (c/b/d): ");
            var input = Console.ReadLine()?.Trim().ToLower();
            switch (input)
            {
                case "c":
                    return 75;
                case "b":
                    return 85;
                case "d":
                    return 95;
                default:
                    Console.WriteLine("Invalid choice. Please enter 'c', 'b', or 'd'.");
                    return null;
            }
        }
    }
}
