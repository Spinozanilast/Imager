using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Imager.ImageConverters;


public class HalftoneToBinaryConverter : IImageConverter
{
    public BitmapSource Convert(BitmapSource source, ImageType? srcImageType)
    {
        if (srcImageType == ImageType.Binary)
        {
            throw new ArgumentException("Изображение уже бинарное.");
        }

        if (srcImageType == ImageType.Color)
        {
            var colorToHalftoneConverter = new ColorToHalftoneConverterHsb();
            source = colorToHalftoneConverter.Convert(source, null);
        }

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
        var pixelData = new byte[height * stride];
        source.CopyPixels(pixelData, stride, 0);

        var binaryData = new byte[height * stride];
        for (var i = 0; i < pixelData.Length; i++)
        {
            binaryData[i] = (byte)(pixelData[i] > 120 ? 255 : 0); 
        }

        var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, binaryData, stride);
        return result;
    }
}