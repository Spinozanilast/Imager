using System.Drawing;
using System.Drawing.Imaging;

namespace Imager;

public class ImageFilterProvider
{
    /// <summary>
    /// Get the Filter string for all supported image types.
    /// To be used in the FileDialog class Filter Property.
    /// </summary>
    /// <returns></returns>
    public static string GetImageFilter()
    {
        var imageExtensions = string.Empty;
        var separator = "";
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
        Dictionary<string, string> imageFilters = new Dictionary<string, string>();

        foreach (var codec in codecs)
        {
            imageExtensions = $"{imageExtensions}{separator}{codec.FilenameExtension.ToLower()}";
            separator = ";";
            imageFilters.Add($"{codec.FormatDescription} files ({codec.FilenameExtension.ToLower()})",
                codec.FilenameExtension.ToLower());
        }

        var result = string.Empty;
        separator = "";

        foreach (var filter in imageFilters)
        {
            result += $"{separator}{filter.Key}|{filter.Value}";
            separator = "|";
        }

        if (!string.IsNullOrEmpty(imageExtensions))
        {
            result += $"{separator}Image files|{imageExtensions}";
        }

        return result;
    }
}