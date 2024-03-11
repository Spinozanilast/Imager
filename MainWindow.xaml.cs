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
using OxyPlot;


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

        /// <summary>
        /// Конструктор главного окна.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Обработчик события загрузки главного окна.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _gridViews = new[] { View2, View3, View4 };
        }

        /// <summary>
        /// Обработчик события изменения выбранного элемента в ComboBox.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
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

        /// <summary>
        /// Изменяет режим отображения изображения.
        /// </summary>
        /// <param name="stretch">Режим отображения изображения.</param>
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

        /// <summary>
        /// Обработчик события нажатия кнопки "Открыть файл".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
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

        /// <summary>
        /// Устанавливает фоновые изображения для представлений.
        /// </summary>
        /// <param name="firstViewBackgroundImage">Фоновое изображение для первого представления.</param>
        private void SetViewsBackgrounds(BitmapImage firstViewBackgroundImage)
        {
            _imageChannelSplitter = new ImageChannelSplitter(firstViewBackgroundImage);
            _imageChannelSplitter.ProcessBitmapSource();

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

        /// <summary>
        /// Обработчик события нажатия кнопки "Очистить все представления".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void ClearAllViewsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllViews();
        }

        private void ClearAllViews()
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

        /// <summary>
        /// Обработчик события переключения режима отображения изображения.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void ImageViewModeToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ModernToggleSwitch)sender;
            GreyScaleGrid.Visibility = toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
            SetImageViewsMode(toggleSwitch.IsOn);
        }

        /// <summary>
        /// Устанавливает режим отображения изображений.
        /// </summary>
        /// <param name="toggleSwitchIsOn">Состояние переключателя.</param>
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

        /// <summary>
        /// Отображает матрицу в DataGrid.
        /// </summary>
        /// <param name="dataGrid">DataGrid для отображения матрицы.</param>
        /// <param name="matrix">Матрица для отображения.</param>
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

        /// <summary>
        /// Обработчик события окончания редактирования ячейки в GreyScaleGrid.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void GreyScaleGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var newValue = ((TextBox)e.EditingElement).Text;
            newValue = string.IsNullOrEmpty(newValue) ? "0" : newValue;
            _imageWriteableProcessor.UpdateBlackAndWhite(e.Column.DisplayIndex, e.Row.GetIndex(),
                byte.Parse(newValue), View1);
            _imageChannelSplitter.UpdateMatrix(ChannelType.RedChannel, e.Column.DisplayIndex, e.Row.GetIndex(), byte.Parse(newValue), true);
            _imageChannelSplitter.OriginalImage = BitmapSourceFromBrush((ImageBrush)View1.Background);
        }

        /// <summary>
        /// Обработчик события окончания редактирования ячейки в DataGridBlue.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void DataGridBlue_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.BlueChannel);
        }

        /// <summary>
        /// Обработчик события окончания редактирования ячейки в DataGridGreen.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void DataGridGreen_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.GreenChannel);
        }

        /// <summary>
        /// Обработчик события окончания редактирования ячейки в DataGridRed.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void DataGridRed_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridCellEditEnding(sender, e, ChannelType.RedChannel);
        }

        /// <summary>
        /// Обработчик события окончания редактирования ячейки в DataGrid.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        /// <param name="channelType">Тип канала.</param>
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

        /// <summary>
        /// Обработчик события нажатия кнопки "Сохранить изображение".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var imageSaver = new ImageSaver(); 
            imageSaver.SaveImageBrushToFile((ImageBrush)View1.Background);
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Открыть представление гистограммы".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
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

        /// <summary>
        /// Создает BitmapSource из Brush.
        /// </summary>
        /// <param name="drawingBrush">Brush для преобразования.</param>
        /// <param name="size">Размер изображения.</param>
        /// <param name="dpi">Разрешение изображения.</param>
        /// <returns>BitmapSource, созданный из Brush.</returns>
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

        /// <summary>
        /// Обработчик события нажатия кнопки "Обновить матрицу в оттенках серого".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
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

        private void ShadingWindowOpenButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            if (_imageChannelSplitter == null || !_imageChannelSplitter.IsBinary)
            {
                MessageBox.Show(this, "Image is not binary");
                return;
            }

            if (!ImageViewModeToggle.IsOn)
            {
                MessageBox.Show(this, "Turn on Matrix Mode");
                return;
            }

            var shadingWindow = new ShadingWindow(_imageChannelSplitter.ChannelMatrices.GreyScaleMatrix);
            shadingWindow.ReturnImage += HandleSelectedImage;
            shadingWindow.Show();
        }

        private void HandleSelectedImage(BitmapImage image)
        {
            ClearAllViews();
            SetViewsBackgrounds(image);
            ImageViewModeToggle.IsEnabled = true;
        }
    }
}