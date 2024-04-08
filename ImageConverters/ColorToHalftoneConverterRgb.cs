using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Imager.ImageConverters;

public class ColorToHalftoneConverterRgb : IImageConverter
{
    public BitmapSource Convert(BitmapSource source, ImageType? srcImageType)
    {
        if (srcImageType == ImageType.Halftone)
            throw new ArgumentException("Изображение уже полутоновое.");

        if (srcImageType == ImageType.Binary)
            throw new ArgumentException("Изображение должно быть цветным, сейчас же оно бинарное, иначе используйте растушёвку.");

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
        var pixelData = new byte[height * stride];
        source.CopyPixels(pixelData, stride, 0);

        var grayscaleData = new byte[height * stride];
        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var average = (byte)((pixelData[i] + pixelData[i + 1] + pixelData[i + 2]) / 3);
            grayscaleData[i] = average;
            grayscaleData[i + 1] = average;
            grayscaleData[i + 2] = average;
            grayscaleData[i + 3] = 255;
        }

        var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, grayscaleData, stride);
        return result;
    }
}