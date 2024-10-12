using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageChannelSplitter.Implementations;
using Imager.Converters;
using Imager.Core;
using Imager.Processors;
using Imager.Services;
using Imager.Utils;
using Microsoft.Win32;
using ModernWpf.Controls;
using GridView = ModernWpf.Controls.GridView;

namespace Imager.Controls;

public partial class ImageTabControl : UserControl
{
    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(ImageTabControl),
            new PropertyMetadata(true));

    private readonly ContentDialog _contentDialog;

    public bool IsAnimationEnabled
    {
        get { return (bool)GetValue(IsAnimationEnabledProperty); }
        set { SetValue(IsAnimationEnabledProperty, value); }
    }

    private readonly ImageRgbChannelsSplitter? _imageRgbChannelsSplitter;

    public ImageRgbChannelsSplitter? ImageChannelSplitter => _imageRgbChannelsSplitter;

    public ImageTabControl()
    {
        InitializeComponent();

        _contentDialog = new ContentDialog()
        {
            Title = "Halftone issues",
            Content =
                "Image that you wanna use for texturing is not halftone. Do you wanna convert it to halftone or just select another image?",
            PrimaryButtonText = "Select another image",
            SecondaryButtonText = "Convert"
        };

        _imageRgbChannelsSplitter = new ImageRgbChannelsSplitter();
    }

    private async void MainView_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an image to build matrix of",
            Multiselect = false,
            Filter = ImageFilterProvider.GetImageFilter()
        };

        var dialogResult = dialog.ShowDialog();

        if (dialogResult == null || !(bool)dialogResult) return;

        var image = new BitmapImage(new Uri(dialog.FileName));

        var imageType = ImageTypeDefiner.DetermineImageType(image).Item1;
        
        if (imageType != ImageType.Halftone)
        {
            var result = await _contentDialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None or ContentDialogResult.Primary:
                    return;
                case ContentDialogResult.Secondary:
                {
                    var converter = new ColorToHalftoneConverterRgb();
                    BitmapImage bitmapImage;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(converter.Convert(image, imageType)));
                        encoder.Save(memoryStream);
                        memoryStream.Position = 0;

                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                    }

                    image = bitmapImage;
                    break;
                }
            }
        }

        MainView.Background = new ImageBrush(image);
        var brush = (MainView.Background as ImageBrush);
        if (brush is not null) brush.Stretch = Stretch.Uniform;

        _imageRgbChannelsSplitter.ProcessBitmapSource(image);
        IsAnimationEnabled = false;
    }
}