using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Imager.Core.GreyscaleChannel;
using Imager.Processors.DistanceCalculators;
using Imager.Processors.ShadingProcessor;
using Imager.Utils;

namespace Imager.Windows
{
    /// <summary>
    /// Interaction logic for ShadingWindow.xaml
    /// </summary>
    public partial class ShadingWindow : Window, IImageReturn
    {
        public event Action<BitmapImage, bool> ReturnImage;

        public ShadingWindow(int[,] grayScaleMatrix)
        {
            InitializeComponent();
            InitDistanceMatrix(grayScaleMatrix);
        }

        private void InitDistanceMatrix(int[,] grayScaleMatrix)
        {
            var manhattanCalculator = new ManhattanDistanceCalculator(grayScaleMatrix);
            var shadingProcessor = new ShadingProcessor(manhattanCalculator);
            var distanceMatrix = shadingProcessor.CalculateManhattanDistance();
            var dataTable = new DataTable();

            for (var i = 0; i < distanceMatrix.GetLength(1); i++)
            {
                dataTable.Columns.Add(i.ToString());
            }

            for (var i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                var row = dataTable.NewRow();
                for (var j = 0; j < distanceMatrix.GetLength(1); j++)
                {
                    row[j] = distanceMatrix[i, j];
                }

                dataTable.Rows.Add(row);
            }

            DistanceMatrixGridView.ItemsSource = dataTable.DefaultView;
            ImagePreview.Source = manhattanCalculator.ConvertToRgbBitmapSource(distanceMatrix);
        }

        private void ReturnImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var source = ImagePreview.Source;

            BitmapImage bitmapImage;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)source));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            ReturnImage?.Invoke(bitmapImage, true);
        }
    }
}