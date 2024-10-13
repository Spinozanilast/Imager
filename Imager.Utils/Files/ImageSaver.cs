using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imager.Utils.FormatConverters;
using Microsoft.Win32;

namespace Imager.Utils.Files;

public class ImageSaver
{
    /// <summary>
    /// Сохраняет ImageBrush в файл.
    /// </summary>
    /// <param name="imageBrush">ImageBrush для сохранения.</param>
    public void SaveImageBrushToFile(ImageBrush imageBrush)
    {
        BitmapImage bitmapImage = ImageBrushToBitmapImageConverter.ConvertImageBrushToBitmapImage(imageBrush);

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Выберите путь для сохранения матрицы в изображение",
            Filter = "JPEG Image|*.jpg|PNG Image|*.png|BMP Image|*.bmp"
        };

        if (saveFileDialog.ShowDialog() != true) return;
        var encoder = GetEncoder(Path.GetExtension(saveFileDialog.FileName));
        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

        using FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
        encoder.Save(fileStream);
    }



    // <summary>
    /// Возвращает кодировщик BitmapEncoder, соответствующий указанному расширению файла.
    /// </summary>
    /// <param name="extension">Расширение файла.</param>
    /// <returns>Кодировщик BitmapEncoder.</returns>
    private BitmapEncoder GetEncoder(string extension)
    {
        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return new JpegBitmapEncoder();
            case ".png":
                return new PngBitmapEncoder();
            case ".bmp":
                return new BmpBitmapEncoder();
            case ".gif":
                return new GifBitmapEncoder();
            case ".tiff":
                return new TiffBitmapEncoder();
            default:
                throw new InvalidOperationException("Неподдерживаемое расширение файла.");
        }
    }
}