using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Imager.Processors;

/// <summary>
/// Класс для разделения изображения на отдельные каналы цвета.
/// </summary>
public class ImageChannelSplitter
{
    /// <summary>
    /// Исходное изображение.
    /// </summary>
    public BitmapSource OriginalImage { get; set; }
    public BitmapSource RedChannel { get; private set; }
    public BitmapSource GreenChannel { get; private set; }
    public BitmapSource BlueChannel { get; private set; }
    public ChannelMatrices ChannelMatrices { get; private set; }

    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="source">Исходное изображение.</param>
    public ImageChannelSplitter(BitmapSource source)
    {
        OriginalImage = source;
        ChannelMatrices = new ChannelMatrices();
        InitializeOutputData(source);
    }

    private void InitializeOutputData(BitmapSource source)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * ((source.Format.BitsPerPixel + 7) / 8);

        var pixelData = new byte[height * stride];
        source.CopyPixels(pixelData, stride, 0);

        ChannelMatrices.RedMatrix = new int[OriginalImage.PixelHeight, OriginalImage.PixelWidth];
        ChannelMatrices.GreenMatrix = new int[OriginalImage.PixelHeight, OriginalImage.PixelWidth];
        ChannelMatrices.BlueMatrix = new int[OriginalImage.PixelHeight, OriginalImage.PixelWidth];
        ChannelMatrices.GreyScaleMatrix = new int[OriginalImage.PixelHeight, OriginalImage.PixelWidth];

        RedChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.RedChannel);
        GreenChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.GreenChannel);
        BlueChannel = CreateBitmapSource(pixelData, stride, width, height, ChannelType.BlueChannel);
    }

    public BitmapSource CreateBitmapSource(byte[] pixelData, int stride, int width, int height, ChannelType channelType)
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
                    ChannelMatrices.RedMatrix[row, col] = pixelData[i - 1];
                    break;
                case ChannelType.GreenChannel:
                    outputPixels[i - 2] = pixelData[i - 2];
                    ChannelMatrices.GreenMatrix[row, col] = pixelData[i - 2];
                    break;
                case ChannelType.BlueChannel:
                    outputPixels[i - 3] = pixelData[i - 3];
                    ChannelMatrices.BlueMatrix[row, col] = pixelData[i - 3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null);
            }

            FillGrayscaleMatrix(pixelData, i, row, col);
        }
        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, outputPixels, stride);
    }

    public void UpdateGreyScaleMatrix(BitmapSource source)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * ((source.Format.BitsPerPixel + 7) / 8);

        var pixelData = new byte[height * stride];
        source.CopyPixels(pixelData, stride, 0);

        for (var i = 3; i < pixelData.Length; i += 4)
        {
            var row = (i / 4) / width;
            var col = (i / 4) % width;

            FillGrayscaleMatrix(pixelData, i, row, col);
        }
    }

    private void FillGrayscaleMatrix(byte[] pixelData, int i, int row, int col)
    {
        if (pixelData[i - 1] == pixelData[i - 2] && pixelData[i - 1] == pixelData[i - 3])
        {
            if (pixelData[i - 1] == 0)
            {
                ChannelMatrices.GreyScaleMatrix[row, col] = 1;
            }
            else
            {
                ChannelMatrices.GreyScaleMatrix[row, col] = 0;
            }
        }
        else
        {
            ChannelMatrices.GreyScaleMatrix[row, col] = 0;
        }
    }

    public void UpdateMatrix(ChannelType channelType, int row, int col, byte newValue, bool isGrayValueUpdate = false)
    {
        if (isGrayValueUpdate)
        {
            ChannelMatrices.GreyScaleMatrix[row, col] = newValue;
        }
        else
        {
            switch (channelType)
            {
                case ChannelType.RedChannel:
                    ChannelMatrices.RedMatrix[row, col] = newValue;
                    break;
                case ChannelType.GreenChannel:
                    ChannelMatrices.GreenMatrix[row, col] = newValue;
                    break;
                case ChannelType.BlueChannel:
                    ChannelMatrices.BlueMatrix[row, col] = newValue;
                    break;
            }

            if (ChannelMatrices.RedMatrix[row, col] != ChannelMatrices.GreenMatrix[row, col] ||
                ChannelMatrices.RedMatrix[row, col] != ChannelMatrices.BlueMatrix[row, col]) return;

            if (ChannelMatrices.RedMatrix[row, col] == 0)
            {
                ChannelMatrices.GreyScaleMatrix[row, col] = 1;
            }
            else
            {
                ChannelMatrices.GreyScaleMatrix[row, col] = 0;
            }
        }
    }
}

public class ChannelMatrices
{
    public int[,] RedMatrix { get; set; }
    public int[,] GreenMatrix { get; set; }
    public int[,] BlueMatrix { get; set; }
    public int[,] GreyScaleMatrix { get; set; }
}