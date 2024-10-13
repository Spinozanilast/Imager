using System.Windows.Media.Imaging;

namespace ImageChannelSplitter;

public interface IImageChannelSplitter
{
    BitmapSource OriginalImage { get; set; }
    BitmapSource RedChannel { get; }
    BitmapSource GreenChannel { get; }
    BitmapSource BlueChannel { get; }
    void ProcessBitmapSource(BitmapSource sourceImage);
}