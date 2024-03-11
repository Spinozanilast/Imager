using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Imager.Processors;

public class ShadingProcessor(int[,] grayScaleMatrix)
{
    private readonly int[,] _grayScaleMatrix = grayScaleMatrix;

    public int[,] CalculateManhattanDistance()
    {
        var width = _grayScaleMatrix.GetLength(0) + 2;
        var height = _grayScaleMatrix.GetLength(1) + 2;
        var distanceMatrix = new int[width, height];
        var maxDistance = width * height;

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                distanceMatrix[i, j] = maxDistance;
            }
        }

        for (var i = 1; i < width - 1; i++)
        {
            for (var j = 1; j < height - 1; j++)
            {
                var isBlack = _grayScaleMatrix[i - 1, j - 1] != 0;
                if (isBlack)
                {
                    distanceMatrix[i, j] = 0;
                }
            }
        }

        for (var i = 1; i < width - 1; i++)
        {
            for (var j = 1; j < height - 1; j++)
            {
                var minDistance = Math.Min(
                    Math.Min(distanceMatrix[i - 1, j - 1], distanceMatrix[i - 1, j]),
                    Math.Min(distanceMatrix[i - 1, j + 1], distanceMatrix[i, j - 1])
                );
                distanceMatrix[i, j] = Math.Min(distanceMatrix[i, j], minDistance + 1);
            }
        }

        for (var i = width - 2; i > 0; i--)
        {
            for (var j = height - 2; j > 0; j--)
            {
                var minDistance = Math.Min(
                    Math.Min(distanceMatrix[i + 1, j + 1], distanceMatrix[i + 1, j]),
                    Math.Min(distanceMatrix[i + 1, j - 1], distanceMatrix[i, j + 1])
                );
                distanceMatrix[i, j] = Math.Min(distanceMatrix[i, j], minDistance + 1);
            }
        }

        var resultMatrix = new int[width - 2, height - 2];
        for (var i = 0; i < width - 2; i++)
        {
            for (var j = 0; j < height - 2; j++)
            {
                resultMatrix[i, j] = distanceMatrix[i + 1, j + 1];
            }
        }

        return resultMatrix;
    }

    public ImageSource ConvertToRgbImageSource(int[,] matrix)
    {
        var width = matrix.GetLength(0);
        var height = matrix.GetLength(1);
        var pixels = new byte[width * height * 3];

        var maxVal = matrix.Cast<int>().Max();
        var minVal = matrix.Cast<int>().Min();

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var index = (j * width + i) * 3;
                var normalizedVal = (matrix[i, j] - minVal) / (double)(maxVal - minVal);
                var val = (byte)(normalizedVal * 255);
                pixels[index] = val;
                pixels[index + 1] = val;
                pixels[index + 2] = val;
            }
        }

        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, null, pixels, width * 3);
    }


}