using System.Windows.Media.Imaging;

namespace Imager.Utils;

public interface IImageReturn
{
    public event Action<BitmapImage, bool> ReturnImage;
}