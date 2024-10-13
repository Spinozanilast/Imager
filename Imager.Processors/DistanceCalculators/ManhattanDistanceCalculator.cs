using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imager.Core.GreyscaleChannel;

namespace Imager.Processors.DistanceCalculators;

public class ManhattanDistanceCalculator : IDistanceCalculator
{
    private readonly int[,] _grayScaleMatrix;

    public ManhattanDistanceCalculator(int[,] grayScaleMatrix)
    {
        _grayScaleMatrix = grayScaleMatrix;
    }

    public int[,] CalculateDistance(int[,] matrix)
    {
        int width = matrix.GetLength(0) + 2;
        int height = matrix.GetLength(1) + 2;
        int maxDistance = width * height;

        var blackDistanceMatrix = InitializeDistanceMatrix(width, height, maxDistance);
        var whiteDistanceMatrix = InitializeDistanceMatrix(width, height, maxDistance);

        CalculateDistances(_grayScaleMatrix, blackDistanceMatrix, whiteDistanceMatrix, maxDistance);

        return GenerateResultMatrix(width, height, blackDistanceMatrix, whiteDistanceMatrix);
    }

    private int[,] InitializeDistanceMatrix(int width, int height, int initialValue)
    {
        var distanceMatrix = new int[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                distanceMatrix[i, j] = initialValue;
            }
        }

        return distanceMatrix;
    }

    private void CalculateDistances(int[,] matrix, int[,] blackDistances, int[,] whiteDistances, int maxDistance)
    {
        int width = matrix.GetLength(0) + 2;
        int height = matrix.GetLength(1) + 2;

        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                bool isBlack = matrix[i - 1, j - 1] != 0;

                blackDistances[i, j] = isBlack ? 0 : maxDistance;
                whiteDistances[i, j] = isBlack ? maxDistance : 0;

                int minBlackDistance = Math.Min(blackDistances[i - 1, j], blackDistances[i, j - 1]);
                int minWhiteDistance = Math.Min(whiteDistances[i - 1, j], whiteDistances[i, j - 1]);

                blackDistances[i, j] = Math.Min(blackDistances[i, j], minBlackDistance + 1);
                whiteDistances[i, j] = Math.Min(whiteDistances[i, j], minWhiteDistance + 1);
            }
        }

        for (int i = width - 2; i > 0; i--)
        {
            for (int j = height - 2; j > 0; j--)
            {
                int minBlackDistance = Math.Min(blackDistances[i + 1, j], blackDistances[i, j + 1]);
                int minWhiteDistance = Math.Min(whiteDistances[i + 1, j], whiteDistances[i, j + 1]);

                blackDistances[i, j] = Math.Min(blackDistances[i, j], minBlackDistance + 1);
                whiteDistances[i, j] = Math.Min(whiteDistances[i, j], minWhiteDistance + 1);
            }
        }
    }

    private int[,] GenerateResultMatrix(int width, int height, int[,] blackDistances, int[,] whiteDistances)
    {
        int[,] resultMatrix = new int[width - 2, height - 2];

        for (int i = 0; i < width - 2; i++)
        {
            for (int j = 0; j < height - 2; j++)
            {
                bool isBlack = _grayScaleMatrix[i, j] != 0;
                resultMatrix[i, j] = isBlack ? whiteDistances[i + 1, j + 1] : -1 * blackDistances[i + 1, j + 1];
            }
        }

        return resultMatrix;
    }

    public BitmapSource ConvertToRgbBitmapSource(int[,] matrix)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        int stride = width * 3; // 3 bytes per pixel in RGB
        byte[] pixels = new byte[height * stride];

        List<int> uniqueValues = matrix.Cast<int>().Distinct().OrderByDescending(v => v).ToList();
        Dictionary<int, byte> valuesDict = GenerateColorDictionary(uniqueValues);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int index = (i * stride) + (j * 3);

                if (valuesDict.TryGetValue(matrix[i, j], out byte colorValue))
                {
                    pixels[index] = colorValue;
                    pixels[index + 1] = colorValue;
                    pixels[index + 2] = colorValue;
                }
            }
        }

        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, null, pixels, stride);
    }

    private Dictionary<int, byte> GenerateColorDictionary(List<int> uniqueValues)
    {
        Dictionary<int, byte> valuesDict = new Dictionary<int, byte>();
        byte step = (byte)(255 / (uniqueValues.Count - 1));

        for (int index = 0; index < uniqueValues.Count; index++)
        {
            byte colorValue = (byte)(byte.MinValue + index * step);
            valuesDict.Add(uniqueValues[index], colorValue);
        }

        return valuesDict;
    }
}