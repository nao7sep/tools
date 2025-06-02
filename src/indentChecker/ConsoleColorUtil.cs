namespace indentChecker
{
    /// <summary>
    /// Provides static methods for colored console output.
    /// </summary>
    public static class ConsoleColorUtil
    {
        public static void Write(string message, ConsoleColor foreground, ConsoleColor? background = null)
        {
            var prevFg = Console.ForegroundColor;
            var prevBg = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            if (background.HasValue)
                Console.BackgroundColor = background.Value;
            Console.Write(message);
            Console.ForegroundColor = prevFg;
            if (background.HasValue)
                Console.BackgroundColor = prevBg;
        }

        public static void WriteLine(string message, ConsoleColor foreground, ConsoleColor? background = null)
        {
            var prevFg = Console.ForegroundColor;
            var prevBg = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            if (background.HasValue)
                Console.BackgroundColor = background.Value;
            Console.WriteLine(message);
            Console.ForegroundColor = prevFg;
            if (background.HasValue)
                Console.BackgroundColor = prevBg;
        }
    }
}
