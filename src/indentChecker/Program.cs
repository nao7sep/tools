﻿namespace indentChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, World!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
