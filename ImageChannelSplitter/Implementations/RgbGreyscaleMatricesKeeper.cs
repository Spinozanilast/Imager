using Imager.Core.GreyscaleChannel;

namespace ImageChannelSplitter.Implementations;

public class RgbGreyscaleMatricesKeeper : IChannelMatricesKeeper
{
    public int[,] RedMatrix { get; set; }
    public int[,] GreenMatrix { get; set; }
    public int[,] BlueMatrix { get; set; }
    public int[,] GrayScaleMatrix { get; set; }
    public int[,] HalftoneMatrix { get; set; }
}