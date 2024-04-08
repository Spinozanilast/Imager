using System.Windows.Media.Imaging;

namespace Imager.Processors;

public static class ImageTypeDefiner
{
    public static Tuple<ImageType, string> DetermineImageType(BitmapSource bitmap)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
        int dataSize = height * stride;
        byte[] pixelData = new byte[dataSize];
        bitmap.CopyPixels(pixelData, stride, 0);

        // Calculate unique pixel values
        var uniquePixelValues = new HashSet<byte>();

        // Check if all channels have the same value
        bool isGrayscale = true;

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte red = pixelData[i];
            byte green = pixelData[i + 1];
            byte blue = pixelData[i + 2];

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