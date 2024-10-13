using Imager.Core;

namespace Imager.Converters;

public class ImageConverterFactory
{
    public static IImageConverter GetConverter(ImageType imageType, ColorToHalftoneCoersionFromType? coersionType)
    {
        switch (imageType)
        {
            case ImageType.Halftone:
                IImageConverter imageConverter = coersionType == ColorToHalftoneCoersionFromType.Rgb
                    ? new ColorToHalftoneConverterRgb()
                    : new ColorToHalftoneConverterHsb();
                return imageConverter;
            case ImageType.Binary:
                return new HalftoneToBinaryConverter();
            default:
                throw new ArgumentException("Unsupported image type.");
        }
    }
}