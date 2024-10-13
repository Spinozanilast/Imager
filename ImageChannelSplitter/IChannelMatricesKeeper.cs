using Imager.Core.GreyscaleChannel;

namespace ImageChannelSplitter;

public interface IChannelMatricesKeeper
{
    int[,] RedMatrix { get; set; }
    int[,] GreenMatrix { get; set; }
    int[,] BlueMatrix { get; set; }
    int[,] GrayScaleMatrix { get; set; }
    int[,] HalftoneMatrix { get; set; }
}