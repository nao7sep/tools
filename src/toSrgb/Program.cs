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
                // JPEG Quality Classification for Adobe RGB to sRGB Conversion
                //
                // This app converts JPEG images from Adobe RGB to sRGB color space,
                // and re-saves them using a user-selected JPEG quality level.
                //
                // Source Analysis:
                // - Sony Alpha (DSLR): original JPEGs align with quality ~96–97
                // - Xiaomi Phone: original JPEGs align with quality ~95–96
                // - ScanSnap Scanner: original JPEGs align with quality ~75–80
                //
                // Purpose:
                // Simplify JPEG output quality selection using three intuitive presets,
                // avoiding vague or misleading terms like "high" or "maximum".
                // These presets reflect practical tradeoffs between file size and visual fidelity.
                //
                // Quality Presets:
                // 1 - Compact  (Quality 75)  : Smaller file size with acceptable visual quality.
                //                              Suitable for sharing, previews, or when storage matters.
                // 2 - Balanced (Quality 85)  : A compromise between quality and size.
                //                              Ideal for everyday use where quality still matters.
                // 3 - Detailed (Quality 95)  : Very low compression artifacts.
                //                              Best for post-processing output, printing, or preserving detail.
                //
                // Note: JPEG quality 100 is intentionally excluded due to its excessive file size
                //       and minimal visual improvement over quality 95.

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
                        var match = Regex.Match(originalFileName, @"^(\d{8}-\d{6})\s+\((.+)\)(.+?)$");
                        string cleanedFileName;
                        if (match.Success)
                        {
                            // match.Groups[0]: full match (entire filename)
                            // match.Groups[1]: timestamp (\d{8}-\d{6})
                            // match.Groups[2]: base name (inside parentheses)
                            // match.Groups[3]: extension (after parentheses)
                            string timestampPart = match.Groups[1].Value;
                            string baseName = match.Groups[2].Value;
                            string ext = match.Groups[3].Value;
                            cleanedFileName = $"{baseName}{ext}";
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
