using System.Text.RegularExpressions;
using ImageMagick;

namespace toSrgb
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = CommandLineOptions.Parse(args);

                if (options.ImagePaths.Count == 0)
                {
                    Console.WriteLine("Usage: toSrgb --quality NN <image1> [image2 ...]");
                    return;
                }

                while (!options.JpegQuality.HasValue)
                {
                    options.JpegQuality = JpegQualitySelector.PromptForQuality();
                }

                // Prepare output directory path, but do not create yet
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string qualityPart = $"quality-{options.JpegQuality.Value}";
                string outputDir = Path.Combine(desktopPath, $"toSrgb-{timestamp}-{qualityPart}");

                foreach (var inputPath in options.ImagePaths)
                {
                    try
                    {
                        if (!File.Exists(inputPath))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"File not found: {inputPath}");
                            Console.ResetColor();
                            continue;
                        }

                        string originalFileName = Path.GetFileName(inputPath);
                        var match = Regex.Match(originalFileName, @"^(\d{8}-\d{6}) \((.+)\)(.+?)$");
                        string cleanedFileName;
                        if (match.Success)
                        {
                            string nameWithoutExt = match.Groups[1].Value;
                            string ext = match.Groups[2].Value;
                            cleanedFileName = $"{nameWithoutExt}{ext}";
                        }
                        else
                        {
                            cleanedFileName = originalFileName;
                        }

                        string outputPath = Path.Combine(
                            outputDir,
                            cleanedFileName);

                        using var image = new MagickImage(inputPath);
                        bool stripMetadata = options.JpegQuality.Value != 95;
                        ImageConverter.ConvertToSrgb(image, stripMetadata: stripMetadata);
                        Directory.CreateDirectory(outputDir);
                        ImageConverter.SaveAsJpeg(image, outputPath, options.JpegQuality.Value);

                        Console.WriteLine($"Processed: {cleanedFileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error processing file {Path.GetFileName(inputPath)}: {ex}");
                        Console.ResetColor();
                    }
                }
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
