using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imager.Core;
using Imager.Core.GreyscaleChannel;

namespace ImageChannelSplitter.Implementations;

public class ImageRgbChannelsSplitter : IImageChannelSplitter
{
    public BitmapSource OriginalImage { get; set; }
    public BitmapSource RedChannel { get; private set; }
    public BitmapSource GreenChannel { get; private set; }
    public BitmapSource BlueChannel { get; private set; }
    public IChannelMatricesKeeper ChannelsMatrices { get; private set; }
    public bool IsBinary { get; private set; }

    public ImageRgbChannelsSplitter()
    {
        ChannelsMatrices = new RgbGreyscaleMatricesKeeper();
        IsBinary = false;
    }

    public void ProcessBitmapSource(BitmapSource source)
    {
        OriginalImage = source;

        var width = OriginalImage.PixelWidth;
        var height = OriginalImage.PixelHeight;
        var stride = width * ((OriginalImage.Format.BitsPerPixel + 7) / 8);

        var pixelData = new byte[height * stride];
        OriginalImage.CopyPixels(pixelData, stride, 0);

        ChannelsMatrices.RedMatrix = new int[height, width];
        ChannelsMatrices.GreenMatrix = new int[height, width];
        ChannelsMatrices.BlueMatrix = new int[height, width];
        ChannelsMatrices.HalftoneMatrix = new int[height, width];
        ChannelsMatrices.GrayScaleMatrix = new int[height, width];

        RedChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.RedChannel);
        GreenChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.GreenChannel);
        BlueChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.BlueChannel);
        CreateBitmapSource(pixelData, stride, width, height, ChannelType.HalftoneChannel);
    }

    private BitmapSource CreateBitmapSource(byte[] pixelData, int stride, int width, int height, ChannelType channelType)
    {
        var outputPixels = new byte[height * stride];

        for (var i = 3; i < pixelData.Length; i += 4)
        {
            var row = (i / 4) / width;
            var col = (i / 4) % width;

            outputPixels[i] = pixelData[i];

            switch (channelType)
            {
                case (ChannelType.RedChannel):
                    outputPixels[i - 1] = pixelData[i - 1];
                    ChannelsMatrices.RedMatrix[row, col] = pixelData[i - 1];
                    break;
                case ChannelType.GreenChannel:
                    outputPixels[i - 2] = pixelData[i - 2];
                    ChannelsMatrices.GreenMatrix[row, col] = pixelData[i - 2];
                    break;
                case ChannelType.BlueChannel:
                    outputPixels[i - 3] = pixelData[i - 3];
                    ChannelsMatrices.BlueMatrix[row, col] = pixelData[i - 3];
                    break;
                case ChannelType.HalftoneChannel:
                    ChannelsMatrices.HalftoneMatrix[row, col] = (pixelData[i - 3] + pixelData[i - 2] + pixelData[i - 1]) / 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null);
            }

            FillGrayscaleMatrix(pixelData, i, row, col);
        }

        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, outputPixels, stride);
    }


    private void FillGrayscaleMatrix(byte[] pixelData, int i, int row, int col)
    {
        if (pixelData[i - 1] == pixelData[i - 2] && pixelData[i - 1] == pixelData[i - 3])
        {
            if (pixelData[i - 1] == 0)
            {
                ChannelsMatrices.GrayScaleMatrix[row, col] = 1;
            }
            else
            {
                ChannelsMatrices.GrayScaleMatrix[row, col] = 0;
            }
        }
        else
        {
            ChannelsMatrices.GrayScaleMatrix[row, col] = 0;
            IsBinary = false;
        }
    }

    public void UpdateMatrix(ChannelType channelType, int row, int col, byte newValue, bool isGrayValueUpdate = false)
    {
        if (isGrayValueUpdate)
        {
            ChannelsMatrices.GrayScaleMatrix[row, col] = newValue;
        }
        else
        {
            UpdateColorMatrix(channelType, row, col, newValue);
        }
    }

    private void UpdateColorMatrix(ChannelType channelType, int row, int col, byte newValue)
    {
        switch (channelType)
        {
            case ChannelType.RedChannel:
                ChannelsMatrices.RedMatrix[row, col] = newValue;
                break;
            case ChannelType.GreenChannel:
                ChannelsMatrices.GreenMatrix[row, col] = newValue;
                break;
            case ChannelType.BlueChannel:
                ChannelsMatrices.BlueMatrix[row, col] = newValue;
                break;
        }

        if (ChannelsMatrices.RedMatrix[row, col] != ChannelsMatrices.GreenMatrix[row, col] ||
            ChannelsMatrices.RedMatrix[row, col] != ChannelsMatrices.BlueMatrix[row, col]) return;

        if (ChannelsMatrices.RedMatrix[row, col] == 0)
        {
            ChannelsMatrices.GrayScaleMatrix[row, col] = 1;
        }
        else
        {
            ChannelsMatrices.GrayScaleMatrix[row, col] = 0;
        }
    }
}