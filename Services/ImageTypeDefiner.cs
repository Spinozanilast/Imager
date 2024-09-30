using System.Windows.Media.Imaging;
using Imager.Core;

namespace Imager.Processors;

public static class ImageTypeDefiner
{
    public static Tuple<ImageType, string> DetermineImageType(BitmapSource bitmap)
    {
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
        var dataSize = height * stride;
        var pixelData = new byte[dataSize];
        bitmap.CopyPixels(pixelData, stride, 0);

        var uniquePixelValues = new HashSet<byte>();

        var isGrayscale = true;

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var red = pixelData[i];
            var green = pixelData[i + 1];
            var blue = pixelData[i + 2];

            uniquePixelValues.Add(red);
            uniquePixelValues.Add(green);
            uniquePixelValues.Add(blue);

            if (red != green || green != blue)
            {
                isGrayscale = false;
            }
        }

        if (uniquePixelValues.Count == 2)
        {
            return new Tuple<ImageType, string>(ImageType.Binary, "Бинарное");
        }
        else if (isGrayscale)
        {
            return new Tuple<ImageType, string>(ImageType.Halftone, "Полутоновое");
        }
        else
        {
            return new Tuple<ImageType, string>(ImageType.Color, "Цветное");
        }
    }
}