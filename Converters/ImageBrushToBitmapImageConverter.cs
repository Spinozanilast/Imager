using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Imager.Converters;

public class ImageBrushToBitmapImageConverter
{
    public static BitmapImage ConvertImageBrushToBitmapImage(ImageBrush imageBrush)
    {
        BitmapImage bitmapImage;
        if (imageBrush.ImageSource is WriteableBitmap writeableBitmap)
        {
            using var outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(writeableBitmap));
            enc.Save(outStream);
            bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(outStream.ToArray());
            bitmapImage.EndInit();
        }
        else
        {
            bitmapImage = (BitmapImage)imageBrush.ImageSource;
        }

        return bitmapImage;
    }

    public static BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int size = 32, int dpi = 96)
    {
        // RenderTargetBitmap = строит растровое изображение визуала
        var pixelFormat = PixelFormats.Pbgra32;
        RenderTargetBitmap rtb = new RenderTargetBitmap(size, size, dpi, dpi, pixelFormat);

        // Drawing visual позволяет нам составить графический рисунок
        DrawingVisual drawingVisual = new DrawingVisual();
        using (DrawingContext context = drawingVisual.RenderOpen())
        {
            context.DrawRectangle(drawingBrush, null, new Rect(new Point(), new Size(size, size)));
        }

        rtb.Render(drawingVisual);
        return rtb;
    }

}