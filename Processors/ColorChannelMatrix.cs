using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Imager.Processors;

public class ColorChannelMatrix
{
    public ObservableCollection<ObservableCollection<int>> Matrix { get; set; }

    public ObservableCollection<ObservableCollection<int>> GrayscaleMatrix { get; set; }

    public ColorChannelMatrix(BitmapImage image, Func<Color, int> colorSelector)
    {
        Matrix = new ObservableCollection<ObservableCollection<int>>();
        GrayscaleMatrix = new ObservableCollection<ObservableCollection<int>>();
        var bitmap = new WriteableBitmap(image);
        var stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
        var pixelData = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixelData, stride, 0);

        for (int y = 0; y < bitmap.PixelHeight; y++)
        {
            var row = new ObservableCollection<int>();
            var grayscaleRow = new ObservableCollection<int>();

            for (int x = 0; x < bitmap.PixelWidth; x++)
            {
                var index = y * stride + 4 * x;
                var blue = pixelData[index];
                var green = pixelData[index + 1];
                var red = pixelData[index + 2];
                var alpha = pixelData[index + 3];

                var color = Color.FromArgb(alpha, red, green, blue);
                row.Add(colorSelector(color));

                grayscaleRow.Add(IsGrayscale(color) ? 1 : 0);

            }
            Matrix.Add(row);
            GrayscaleMatrix.Add(grayscaleRow);
        }
    }

    private bool IsGrayscale(Color color)
    {
        return color.R == color.G && color.R == color.B;
    }
}