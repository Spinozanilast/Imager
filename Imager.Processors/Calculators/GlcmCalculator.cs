namespace Imager.Processors.Calculators;

public class GlcmCalculator
{
    public static int[,] CalculateGlcm(int[,] grayscaleMatrix, int distance, int angle, out int gradationsCount)
    {
        var greyHashSet = new HashSet<int>();
        var width = grayscaleMatrix.GetLength(1);
        var height = grayscaleMatrix.GetLength(0);
        var numLevels = 256;
        gradationsCount = 0;

        var glcm = new int[numLevels, numLevels];

        (var offsetX, var offsetY) = GetOffset(distance, angle);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var neighborX = x + offsetX;
                var neighborY = y + offsetY;

                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    var grey = grayscaleMatrix[y, x];
                    greyHashSet.Add(grey);
                    var neighborGrey = grayscaleMatrix[neighborY, neighborX];

                    glcm[grey, neighborGrey]++;
                }
            }
        }

        gradationsCount = greyHashSet.Count;
        return glcm;
    }

    private static (int, int) GetOffset(int distance, int angle)
    {
        return angle switch
        {
            0 => (distance, 0),
            45 => (distance, -distance),
            90 => (0, -distance),
            135 => (-distance, -distance),
            180 => (-distance, 0),
            225 => (-distance, distance),
            270 => (0, distance),
            315 => (distance, distance),
            360 => (distance, 0),
            _ => throw new ArgumentException(
                "Invalid angle. Only 0, 45, 90, 135, 180, 225, 270, 315, and 360 degrees are supported.")
        };
    }
}