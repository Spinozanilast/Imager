﻿using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Wpf;
using System.IO;

namespace Imager.Utils
{
    /// <summary>
    /// Interaction logic for HistogramViewer.xaml
    /// </summary>
    public partial class HistogramViewer : Window
    {
        public HistogramViewer(BitmapImage image)
        {
            InitializeComponent();

            var bitmap = BitmapFromSource(image);

            var redHistogram = GetColorHistogram(bitmap, ChannelType.RedChannel);
            var greenHistogram = GetColorHistogram(bitmap, ChannelType.GreenChannel);
            var blueHistogram = GetColorHistogram(bitmap, ChannelType.BlueChannel);

            DisplayHistogram(redHistogram, RedHistogramPlot, OxyColors.Red, "Red Color Histogram");
            DisplayHistogram(greenHistogram, GreenHistogramPlot, OxyColors.Green, "Green Color Histogram");
            DisplayHistogram(blueHistogram, BlueHistogramPlot, OxyColors.Blue, "Blue Color Histogram");
        }

        private static Bitmap BitmapFromSource(BitmapImage bitmapimage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapimage));
                enc.Save(outStream);
                outStream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(outStream);
            }
        }

        private int[] GetColorHistogram(Bitmap bitmap, ChannelType channel)
        {
            var histogram = new int[256];

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);

                    var colorValue = 0;
                    switch (channel)
                    {
                        case ChannelType.RedChannel:
                            colorValue = color.R;
                            break;
                        case ChannelType.GreenChannel:
                            colorValue = color.G;
                            break;
                        case ChannelType.BlueChannel:
                            colorValue = color.B;
                            break;
                    }

                    histogram[colorValue]++;
                }
            }

            return histogram;
        }

        private void DisplayHistogram(int[] values, PlotView plotView, OxyColor color, String title)
        {
            var model = new PlotModel { Title = title };

            var series = new HistogramSeries()
            {
                FillColor = color,
                StrokeColor = color,
                StrokeThickness = 1
            };

            for (int i = 0; i < values.Length; i++)
            {
                series.Items.Add(new HistogramItem(i, i + 1, values[i], 0));
            }

            model.Series.Add(series);
            plotView.Model = model;
        }
    }
}
