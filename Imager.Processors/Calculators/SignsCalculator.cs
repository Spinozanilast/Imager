using Imager.Utils;

namespace Imager.Processors.Calculators;

public static class SignsCalculator
{
    public static double CalculateEnergy(double[,] matrix, int count)
    {
        var result = 0.0;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                result += Math.Pow(matrix[i, j], 2);
            }
        }

        return result;
    }

    public static double CalculateHomogenity(double[,] matrix, int count)
    {
        var result = 0.0;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                result += Math.Pow(matrix[i, j], 2) / (1 + Math.Abs(i - j));
            }
        }

        return result;
    }

    public static double CalculateContrast(double[,] matrix, int count)
    {
        var result = 0.0;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                result += matrix[i, j] * Math.Pow(i - j,2);
            }
        }

        return result;
    }

    public static double CalculateR(SignsCoefs currentImageCoefs, SignsCoefs mainImageCoefs)
    {
        double result = 0;
        result += Math.Pow(currentImageCoefs.Energy - mainImageCoefs.Energy, 2);
        result += Math.Pow(currentImageCoefs.Contrast - mainImageCoefs.Contrast, 2);
        result += Math.Pow(currentImageCoefs.Homogeinity - mainImageCoefs.Homogeinity, 2);
        return Math.Sqrt(result);
    }
}