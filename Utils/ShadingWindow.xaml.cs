using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Imager.Processors;

namespace Imager.Utils
{
    /// <summary>
    /// Interaction logic for ShadingWindow.xaml
    /// </summary>
    public partial class ShadingWindow : Window
    {

        public event Action<BitmapImage> ReturnImage;

        public ShadingWindow(int[,] grayScaleMatrix)
        {
            InitializeComponent();
            InitDistanceMatrix(grayScaleMatrix);
        }

        private void InitDistanceMatrix(int[,] grayScaleMatrix)
        {
            var shadingProcessor = new ShadingProcessor(grayScaleMatrix);
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
            ImagePreview.Source = shadingProcessor.ConvertToRgbImageSource(distanceMatrix);

        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var imageSaver = new ImageSaver();
            imageSaver.SaveImageBrushToFile(new ImageBrush(ImagePreview.Source));
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
            ReturnImage?.Invoke(bitmapImage);
        }
    }
}
