using ImageMagick;

/// <summary>
/// Provides image processing utilities for converting images to sRGB and saving as JPEG.
/// </summary>
public class ImageConverter
{
    /// <summary>
    /// Prepares the image for sRGB conversion: auto orient, color space transform, remove ICC, remove thumbnail.
    /// Optionally strips metadata.
    /// </summary>
    /// <param name="image">The MagickImage to process.</param>
    /// <param name="stripMetadata">If true, strips all metadata except color profile (default: false).</param>
    public static void ConvertToSrgb(MagickImage image, bool stripMetadata = false)
    {
        // Automatically rotate the image based on its EXIF orientation tag
        image.AutoOrient();

        // If the image has an embedded ICC profile, convert its color space to sRGB and remove the ICC profile
        if (image.HasProfile("icc"))
        {
            // Transform the image's color space to sRGB using the embedded ICC profile
            image.TransformColorSpace(ColorProfile.SRGB);
            // Remove the ICC profile after conversion to reduce file size and avoid color management issues
            image.RemoveProfile("icc");
        }
        // If there is no ICC profile and the color space is not sRGB, assume AdobeRGB and convert to sRGB
        else if (image.ColorSpace != ColorSpace.sRGB)
        {
            // Transform from AdobeRGB (a common camera color space) to sRGB
            image.TransformColorSpace(ColorProfile.AdobeRGB1998, ColorProfile.SRGB);
        }
        // Retrieve the EXIF profile to check for and remove any embedded thumbnail
        var exifProfile = image.GetExifProfile();
        if (exifProfile != null && exifProfile.ThumbnailOffset != 0 && exifProfile.ThumbnailLength != 0)
        {
            // Remove the thumbnail from the EXIF profile to save space and prevent outdated previews
            exifProfile.RemoveThumbnail();
            // Update the image's EXIF profile with the thumbnail removed
            image.SetProfile(exifProfile);
        }
        // Optionally remove all profiles and metadata except color profile (if present) to minimize file size
        if (stripMetadata)
        {
            image.Strip();
        }
    }

    /// <summary>
    /// Saves the image as JPEG at the specified quality.
    /// </summary>
    /// <param name="image">The MagickImage to save.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="quality">JPEG quality (1-100).</param>
    public static void SaveAsJpeg(MagickImage image, string outputPath, int quality)
    {
        // Set the JPEG compression quality (1-100, higher means better quality and larger file)
        image.Quality = (uint)quality;
        // Write the image to disk in JPEG format
        image.Write(outputPath, MagickFormat.Jpeg);
    }
}
