using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageChannelSplitter.Implementations;
using Microsoft.Win32;
using GridView = ModernWpf.Controls.GridView;

namespace Imager.Controls;

public partial class ImageTabControl : UserControl
{
    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(ImageTabControl),
            new PropertyMetadata(true));

    public bool IsAnimationEnabled
    {
        get { return (bool)GetValue(IsAnimationEnabledProperty); }
        set { SetValue(IsAnimationEnabledProperty, value); }
    }

    private readonly ImageRgbChannelsSplitter _imageRgbChannelsSplitter;

    public ImageRgbChannelsSplitter ImageChannelSplitter => _imageRgbChannelsSplitter;

    public ImageTabControl()
    {
        InitializeComponent();
        
        _imageRgbChannelsSplitter = new ImageRgbChannelsSplitter();
    }

    private void MainView_OnMouseDown(object sender, MouseButtonEventArgs e)
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
        MainView.Background = new ImageBrush(image);
        var brush = (MainView.Background as ImageBrush);
        if (brush is not null) brush.Stretch = Stretch.Uniform;
        
        _imageRgbChannelsSplitter.ProcessBitmapSource(image);
        IsAnimationEnabled = false;
    }
}