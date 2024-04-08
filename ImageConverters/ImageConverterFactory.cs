namespace Imager.ImageConverters;

public class ImageConverterFactory
{
    public static IImageConverter GetConverter(ImageType imageType, ColorToHalftoneCoersionType? coersionType)
    {
        switch (imageType)
        {
            case ImageType.Halftone:
                IImageConverter imageConverter = coersionType == ColorToHalftoneCoersionType.Rgb
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