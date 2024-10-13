using Imager.Core.GreyscaleChannel;

namespace Imager.Processors;

public interface IDistanceCalculator
{
    int[,] CalculateDistance(int[,] matrix);
}