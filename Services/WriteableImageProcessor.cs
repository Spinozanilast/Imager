using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imager.Core;

namespace Imager.Services;

public class WriteableImageProcessor
{
    private WriteableBitmap _writeableBitmap;
    private ImageBrush _imageBrush;
    public Stretch ImageStretches { get; set; }

    public WriteableImageProcessor(BitmapImage bitmapSource, ImageBrush imageBrush)
    {
        _writeableBitmap = new WriteableBitmap(bitmapSource);
        _imageBrush = imageBrush;
        ImageStretches = Stretch.Fill;
    }

    public void UpdatePixelChannel(int x, int y, byte newValue, ChannelType channel, ModernWpf.Controls.GridView grid)
    {
        _writeableBitmap.Lock();

        IntPtr buffer = _writeableBitmap.BackBuffer;
        int stride = _writeableBitmap.BackBufferStride;

        unsafe
        {
            byte* pbBuffer = (byte*)buffer.ToPointer();
            int index = y * stride + 4 * x;

            switch (channel)
            {
                case ChannelType.BlueChannel:
                    pbBuffer[index] = newValue;
                    break;
                case ChannelType.GreenChannel:
                    pbBuffer[index + 1] = newValue;
                    break;
                case ChannelType.RedChannel:
                    pbBuffer[index + 2] = newValue;
                    break;
            }
        }

        _writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        _writeableBitmap.Unlock();

        ImageBrush imageBrush = new ImageBrush(_writeableBitmap);
        imageBrush.Stretch = ImageStretches;
        grid.Background = imageBrush;
    }

    public void UpdateBlackAndWhite(int x, int y, byte newValue, ModernWpf.Controls.GridView grid)
    {
        _writeableBitmap.Lock();

        IntPtr buffer = _writeableBitmap.BackBuffer;
        int stride = _writeableBitmap.BackBufferStride;

        unsafe
        {
            byte* pbBuffer = (byte*)buffer.ToPointer();
            int index = y * stride + 4 * x;

            if (newValue == 0)
            {
                pbBuffer[index] = 255;
                pbBuffer[index + 1] = 255;
                pbBuffer[index + 2] = 255;
            }
            else if (newValue == 1)
            {
                pbBuffer[index] = 0;
                pbBuffer[index + 1] = 0;
                pbBuffer[index + 2] = 0;
            }
        }

        _writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        _writeableBitmap.Unlock();

        ImageBrush imageBrush = new ImageBrush(_writeableBitmap);
        imageBrush.Stretch = ImageStretches;
        grid.Background = imageBrush;
    }
}