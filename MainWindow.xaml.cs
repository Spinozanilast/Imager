using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imager.Processors;
using ModernToggleSwitch = ModernWpf.Controls.ToggleSwitch;
using System.Data;
using System.IO;
using Imager.Converters;
using Imager.Utils;
using MessageBox = ModernWpf.MessageBox;


namespace Imager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataGrid[] _gridViews;
        private string _lastFilename = string.Empty;
        private Stretch _currentViewStratch = Stretch.Fill;
        private ImageChannelSplitter _imageChannelSplitter;
        private ImageWriteableProcessor _imageWriteableProcessor;
        private double _maxScale = 20.0;
        private bool _enableZoom = true;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _gridViews = new[] { View2, View3, View4 };
        }

        private void ImageDisplayOption_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOption = (sender as ComboBox).SelectedItem as ComboBoxItem;
            switch (selectedOption.Content)
            {
                case "Растянуть":
                    ChangeImageStretch(Stretch.Fill);
                    if (_imageWriteableProcessor != null) _imageWriteableProcessor.ImageStretches = Stretch.Fill;
                    break;
                case "Нормальный":
                    ChangeImageStretch(Stretch.None);
                    if (_imageWriteableProcessor != null) _imageWriteableProcessor.ImageStretches = Stretch.None;
                    break;
                case "Центрировать":
                    ChangeImageStretch(Stretch.Uniform);
                    if (_imageWriteableProcessor != null) _imageWriteableProcessor.ImageStretches = Stretch.Uniform;
                    break;
                case "Заполнить":
                    ChangeImageStretch(Stretch.UniformToFill);
                    if (_imageWriteableProcessor != null)
                        _imageWriteableProcessor.ImageStretches = Stretch.UniformToFill;
                    break;
                default:
                    ChangeImageStretch(Stretch.None);
                    break;
            }
        }

        private void ChangeImageStretch(Stretch stretch)
        {
            if (_gridViews == null) return;

            foreach (var gridView in _gridViews)
            {
                if (gridView != null && gridView.Background is ImageBrush imageBrushGridViewBackground)
                {
                    imageBrushGridViewBackground.Stretch = stretch;
                }
            }

            if (View1 != null && View1.Background is ImageBrush imageBrush)
            {
                imageBrush.Stretch = stretch;
            }

            _currentViewStratch = stretch;
        }

        private void OpenFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select an image to build matrix of",
                Multiselect = false,
                Filter = ImageFilterProvider.GetImageFilter()
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult == null || !(bool)dialogResult) return;

            _lastFilename = dialog.FileName;
            var image = new BitmapImage(new Uri(_lastFilename));
            SetViewsBackgrounds(image);
            ChangeImageStretch(_currentViewStratch);

            ImageViewModeToggle.IsEnabled = true;
            SaveImageButton.IsEnabled = true;
        }

        private void SetViewsBackgrounds(BitmapImage firstViewBackgroundImage)
        {
            _imageChannelSplitter = new ImageChannelSplitter(firstViewBackgroundImage);

            View1.Background = new ImageBrush(firstViewBackgroundImage);
            View2.Background = new ImageBrush(_imageChannelSplitter.RedChannel);
            View3.Background = new ImageBrush(_imageChannelSplitter.GreenChannel);
            View4.Background = new ImageBrush(_imageChannelSplitter.BlueChannel);

            var st = new ScaleTransform();
            View1.LayoutTransform = st;

            View1.MouseWheel += (sender, e) =>
            {
                if (!_enableZoom) return; 

                if (e.Delta > 0 && st.ScaleX < _maxScale)
                {
                    st.ScaleX *= 1.1;
                    st.ScaleY *= 1.1;
                }
                else if (e.Delta < 0)
                {
                    st.ScaleX /= 1.1;
                    st.ScaleY /= 1.1;
                }
            };

            _imageWriteableProcessor = new ImageWriteableProcessor(firstViewBackgroundImage, View1.Background as ImageBrush);
        }

        private void ClearAllViewsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var view in _gridViews)
            {
                view.Background = null;
                view.ItemsSource = null;
                ImageViewModeToggle.IsOn = false;
            }

            View1.Background = null;
            GreyScaleGrid.ItemsSource = null;

            ImageViewModeToggle.IsEnabled = false;
            SaveImageButton.IsEnabled = false;
        }

        private void ImageViewModeToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ModernToggleSwitch)sender;
            SetImageViewsMode(toggleSwitch.IsOn);
        }

        private void SetImageViewsMode(bool toggleSwitchIsOn)
        {
            switch (toggleSwitchIsOn)
            {
                case true:
                    DisplayMatrixInGridView(View2, _imageChannelSplitter.ChannelMatrices.RedMatrix);
                    DisplayMatrixInGridView(View3, _imageChannelSplitter.ChannelMatrices.GreenMatrix);
                    DisplayMatrixInGridView(View4, _imageChannelSplitter.ChannelMatrices.BlueMatrix);
                    if (GreyScaleGrid.ItemsSource == null)
                    {
                        DisplayMatrixInGridView(GreyScaleGrid, _imageChannelSplitter.ChannelMatrices.GreyScaleMatrix);
                    }
                    break;
                case false:
                    View2.ItemsSource = null;
                    View3.ItemsSource = null;
                    View4.ItemsSource = null;
                    break;
            }
        }

        private void DisplayMatrixInGridView(DataGrid dataGrid, int[,] matrix)
        {
            View2.CellEditEnding += DataGridRed_CellEditEnding;
            View3.CellEditEnding += DataGridGreen_CellEditEnding;
            View4.CellEditEnding += DataGridBlue_CellEditEnding;
            GreyScaleGrid.CellEditEnding += GreyScaleGrid_CellEditEnding;

            var dataTable = new DataTable();

            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                dataTable.Columns.Add(i.ToString());
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var row = dataTable.NewRow();
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row[j] = matrix[i, j];
                }

                dataTable.Rows.Add(row);
            }

            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void GreyScaleGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var newValue = ((TextBox)e.EditingElement).Text;
            newValue = string.IsNullOrEmpty(newValue) ? "0" : newValue;
            _imageWriteableProcessor.UpdateBlackAndWhite(e.Column.DisplayIndex, e.Row.GetIndex(),
                byte.Parse(newValue), View1);
            _imageChannelSplitter.UpdateMatrix(ChannelType.RedChannel, e.Column.DisplayIndex, e.Row.GetIndex(), byte.Parse(newValue), true);
            _imageChannelSplitter.OriginalImage = BitmapSourceFromBrush((ImageBrush)View1.Background);
        }

        private void DataGridBlue_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.BlueChannel);
        }

        private void DataGridGreen_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.GreenChannel);
        }

        private void DataGridRed_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.RedChannel);
        }

        private void DataGridCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e, ChannelType channelType)
        {
            try
            {
                var newValue = ((TextBox)e.EditingElement).Text;
                newValue = string.IsNullOrEmpty(newValue) ? "0" : newValue;
                _imageWriteableProcessor.UpdatePixelChannel(e.Column.DisplayIndex, e.Row.GetIndex(),
                    byte.Parse(newValue),
                    channelType, View1);
                _imageChannelSplitter.UpdateMatrix(channelType, e.Column.DisplayIndex, e.Row.GetIndex(),byte.Parse(newValue));
                _imageChannelSplitter.OriginalImage = BitmapSourceFromBrush((ImageBrush)View1.Background);
            }
            catch (Exception)
            {
                //ModernWpf.MessageBox.Show(this, ex.Message, "Недопустимое значение ячейки");
            }
        }


        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImageBrushToFile((ImageBrush)View1.Background);
        }

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

        private void HistogramViewOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (View1.Background == null)
            {
                MessageBox.Show(this, "Load picture");
                return;
            }
            var bitmapImage =
                ImageBrushToBitmapImageConverter.ConvertImageBrushToBitmapImage((ImageBrush)View1.Background);
            HistogramViewer histogramViewer = new HistogramViewer(ImageBrushToBitmapImageConverter.ConvertImageBrushToBitmapImage((ImageBrush)View1.Background));
            histogramViewer.Show();
        }

        public static BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int size = 32, int dpi = 96)
        {
            var pixelFormat = PixelFormats.Pbgra32;
            RenderTargetBitmap rtb = new RenderTargetBitmap(size, size, dpi, dpi, pixelFormat);

            var drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(drawingBrush, null, new Rect(0, 0, size, size));
            }

            rtb.Render(drawingVisual);
            return rtb;
        }

        private void UpdateGreyscaleMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            if (GreyScaleGrid.ItemsSource == null)
            {
                MessageBox.Show(this, "Initialize matrix view");
                return;
            }
            GreyScaleGrid.ItemsSource = null;
            DisplayMatrixInGridView(GreyScaleGrid, _imageChannelSplitter.ChannelMatrices.GreyScaleMatrix);
        }
    }
}