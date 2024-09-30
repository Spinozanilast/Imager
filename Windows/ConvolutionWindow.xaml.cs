using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imager.Utils
{
    /// <summary>
    /// Interaction logic for ConvolutionWindow.xaml
    /// </summary>
    public partial class ConvolutionWindow : Window, IImageReturn
    {
        public readonly string MaskType = "Median";

        public event Action<BitmapImage, bool> ReturnImage;
        private (int[,] redMatrix, int[,] greenMatrix, int[,] blueMatrix) colorMatrixTuple;

        public ConvolutionWindow(int[,] redMatrix, int[,] greenMatrix, int[,] blueMatrix)
        {
            InitializeComponent();
            colorMatrixTuple = (redMatrix,  greenMatrix, blueMatrix);
        }

        private void ConvoluteImageButton_Click(object sender, RoutedEventArgs e)
        {
            var matrixSize = (int)MatrixDimensionSlider.Value;
            var newRedMatrix = new int[colorMatrixTuple.redMatrix.GetLength(0), colorMatrixTuple.redMatrix.GetLength(1)];
            var newGreenMatrix = new int[colorMatrixTuple.greenMatrix.GetLength(0), colorMatrixTuple.greenMatrix.GetLength(1)];
            var newBlueMatrix = new int[colorMatrixTuple.blueMatrix.GetLength(0), colorMatrixTuple.blueMatrix.GetLength(1)];

            var neighborhood = new List<int>((matrixSize * 2 + 1) * (matrixSize * 2 + 1));

            for (int i = 0; i < colorMatrixTuple.redMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < colorMatrixTuple.redMatrix.GetLength(1); j++)
                {
                    GetNeighborhood(colorMatrixTuple.redMatrix, i, j, matrixSize, neighborhood);
                    newRedMatrix[i, j] = GetMedian(neighborhood);
                    neighborhood.Clear();

                    GetNeighborhood(colorMatrixTuple.greenMatrix, i, j, matrixSize, neighborhood);
                    newGreenMatrix[i, j] = GetMedian(neighborhood);
                    neighborhood.Clear();

                    GetNeighborhood(colorMatrixTuple.blueMatrix, i, j, matrixSize, neighborhood);
                    newBlueMatrix[i, j] = GetMedian(neighborhood);
                    neighborhood.Clear();
                }
            }

            var newImage = MatrixToBitmapImage(newRedMatrix, newGreenMatrix, newBlueMatrix);
            ReturnImage?.Invoke(newImage, false);
        }

        private void GetNeighborhood(int[,] matrix, int x, int y, int size, List<int> neighborhood)
        {
            var offset = size / 2;
            for (int i = -offset; i <= offset; i++)
            {
                for (int j = -offset; j <= offset; j++)
                {
                    var newX = x + i;
                    var newY = y + j;
                    if (newX >= 0 && newX < matrix.GetLength(0) && newY >= 0 && newY < matrix.GetLength(1))
                    {
                        neighborhood.Add(matrix[newX, newY]);
                    }
                }
            }
        }

        private int GetMedian(List<int> values)
        {
            values.Sort();
            return values[values.Count / 2];
        }

        private BitmapImage MatrixToBitmapImage(int[,] redMatrix, int[,] greenMatrix, int[,] blueMatrix)
        {
            var width = redMatrix.GetLength(1);
            var height = redMatrix.GetLength(0);
            var stride = width * ((32 + 7) / 8);
            var pixelData = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var i = y * stride + 4 * x;
                    pixelData[i] = (byte)blueMatrix[y, x];
                    pixelData[i + 1] = (byte)greenMatrix[y, x];
                    pixelData[i + 2] = (byte)redMatrix[y, x];
                    pixelData[i + 3] = 255;
                }
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixelData, stride);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
