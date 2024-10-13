namespace Imager.Processors.Calculators;

public class TexturingCfsCalculator
{
    public double CalculateHomogeneityCf(int[,] matrix, int gradationsNum)
    {
        double res = 0;
        for (int i = 0; i < gradationsNum; i++)
        for (int j = 0; j < gradationsNum; j++)
            res += (Math.Pow(matrix[i, j], 2)) / (1 + Math.Abs(i - j));
        return res;
    }

    public double CalculateContrastCf(int[,] matrix, int gradationsNum)
    {
        double res = 0;
        for (int i = 0; i < gradationsNum; i++)
        for (int j = 0; j < gradationsNum; j++)
            res += matrix[i, j] * (i - j) * (i - j);
        return res;
    }

    public double CalculateEnergyCf(int[,] matrix, int gradationsNum)
    {
        double res = 0;
        for (int i = 0; i < gradationsNum; i++)
        for (int j = 0; j < gradationsNum; j++)
            res += Math.Pow(matrix[i, j], 2);
        return res;
    }

    public void CalculateSimilarityCf(double homoCf, double contrastCf, double glcmMatrix)
    {
        
    }
}