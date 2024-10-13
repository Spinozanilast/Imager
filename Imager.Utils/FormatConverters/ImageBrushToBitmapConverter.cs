using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imager.Utils.FormatConverters;

public class ImageBrushToBitmapImageConverter
{
    /// <summary>
    /// Конвертирует ImageBrush в BitmapImage.
    /// </summary>
    /// <param name="imageBrush">ImageBrush для конвертации.</param>
    /// <returns>BitmapImage, полученный из ImageBrush.</returnsa>
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

    /// <summary>
    /// Создает BitmapSource из Brush.
    /// </summary>
    /// <param name="drawingBrush">Brush для преобразования.</param>
    /// <param name="size">Размер изображения.</param>
    /// <param name="dpi">Разрешение изображения.</param>
    /// <returns>BitmapSource, созданный из Brush.</returns>
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