using System.Windows.Media.Imaging;
using Imager.Core;

namespace Imager.Converters;

public interface IImageConverter
{
    BitmapSource Convert(BitmapSource source, ImageType? srcImageType);
}