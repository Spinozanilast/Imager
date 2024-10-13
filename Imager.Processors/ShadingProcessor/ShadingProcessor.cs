using Imager.Core.GreyscaleChannel;

namespace Imager.Processors.ShadingProcessor;

public class ShadingProcessor: IMatrixProcessor
{
    private readonly int[,] _grayScaleMatrix;
    private readonly IDistanceCalculator _distanceCalculator;

    public ShadingProcessor(IDistanceCalculator distanceCalculator)
    {
        _distanceCalculator = distanceCalculator ?? throw new ArgumentNullException(nameof(distanceCalculator));
    }

    public int[,] CalculateManhattanDistance()
    {
        return _distanceCalculator.CalculateDistance(_grayScaleMatrix);
    }
}