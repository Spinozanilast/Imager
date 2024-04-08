using System.Windows.Media.Imaging;

namespace Imager.ImageConverters;

public interface IImageConverter
{
    BitmapSource Convert(BitmapSource source, ImageType? srcImageType);
}