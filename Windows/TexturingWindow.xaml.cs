using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Imager.Controls;
using Imager.Processors.Calculators;
using Imager.Processors.Processors;

namespace Imager.Windows;

public partial class TexturingWindow : Window
{
    private readonly string[] Cfs = new[] { "Energy", "Contrast", "Homogeinity" };

    private readonly (ImageTabControl imageTabControl, int[,]? glcm)[] _tabControls;
    private readonly DataTableFromMatrixCreator _tableFromMatrixCreator;

    public TexturingWindow(DataTableFromMatrixCreator tableFromMatrixCreator)
    {
        InitializeComponent();
        _tableFromMatrixCreator = tableFromMatrixCreator;
        _tabControls = GetTabControls(MainGrid)
            .Select(tab => (tab, glcm: (int[,]?)null))
            .ToArray();
        InitCfsDataGrid();
    }

    private void InitCfsDataGrid()
    {
        var dataTable = new DataTable();
        for (var i = 0; i < _tabControls.Length; i++)
        {
            dataTable.NewRow();
        }

        for (var i = 0; i < Cfs.Length; i++)
        {
            dataTable.Columns.Add(Cfs[i]);
        }

        CfsDataGrid.ItemsSource = dataTable.DefaultView;
    }

    private void CalculateCoOccurenceMatrices_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValidAngleSelection()) return;

        var angle = GetSelectedAngle();
        var distanceNum = DistanceNumberBox.Value;

        foreach (var tabControlTuple in _tabControls)
        {
            CalculateAndDisplayGlcm(tabControlTuple, (int)distanceNum, angle);
        }
    }

    private bool IsValidAngleSelection()
    {
        var selectedAngleItem = AngleComboBox.SelectedItem as ComboBoxItem;
        return selectedAngleItem != null && int.TryParse(selectedAngleItem.Content.ToString(), out _);
    }

    private int GetSelectedAngle()
    {
        var selectedAngleItem = AngleComboBox.SelectedItem as ComboBoxItem;
        return int.Parse(selectedAngleItem.Content.ToString());
    }

    private void CalculateAndDisplayGlcm((ImageTabControl imageTabControl, int[,]? glcm) imageTabControl,
        int distanceNum, int angle)
    {
        if (imageTabControl.imageTabControl.ImageChannelSplitter?.ChannelsMatrices.HalftoneMatrix is null) return;

        var halftoneMatrix = imageTabControl.imageTabControl.ImageChannelSplitter.ChannelsMatrices.HalftoneMatrix;
        var glcm = GlcmCalculator.CalculateGlcm(halftoneMatrix, distanceNum, angle);

        var dataTable = _tableFromMatrixCreator.ConvertMatrixToDataTable(glcm);
        imageTabControl.imageTabControl.CoOcurrenceMatrix.ItemsSource = dataTable.DefaultView;

        var index = Array.FindIndex(_tabControls, t => t.imageTabControl == imageTabControl.imageTabControl);

        if (index != -1)
        {
            _tabControls[index] = (imageTabControl.imageTabControl, glcm);
        }
    }

    private DataTable CreateDataTableFromGlcm(int[,] glcm)
    {
        var dataTable = new DataTable();
        for (int i = 0; i < glcm.GetLength(1); i++)
        {
            dataTable.Columns.Add(i.ToString());
        }

        for (int i = 0; i < glcm.GetLength(0); i++)
        {
            var row = dataTable.NewRow();
            for (int j = 0; j < glcm.GetLength(1); j++)
            {
                row[j] = glcm[i, j];
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    private void NormalizeMatrices_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var tabControlTuple in _tabControls)
        {
            NormalizeAndDisplayGlcm(tabControlTuple.imageTabControl, tabControlTuple.glcm);
        }
    }

    private void NormalizeAndDisplayGlcm(ImageTabControl imageTabControl, int[,]? glcmMatrix)
    {
        if (glcmMatrix is null)
        {
            MessageBox.Show("Glcm matrix is null");
            return;
        }

        var normalizedDataTable = CreateNormalizedDataTableFromGlcm(glcmMatrix);
        imageTabControl.NormalizedCoOcurrenceMatrix.ItemsSource = normalizedDataTable.DefaultView;
    }

    private DataTable CreateNormalizedDataTableFromGlcm(int[,] glcmMatrix)
    {
        double sum = CalculateSumOfGlcm(glcmMatrix);
        var dataTable = new DataTable();
        for (int i = 0; i < glcmMatrix.GetLength(1); i++)
        {
            dataTable.Columns.Add(i.ToString());
        }

        for (int i = 0; i < glcmMatrix.GetLength(0); i++)
        {
            var row = dataTable.NewRow();

            for (int j = 0; j < glcmMatrix.GetLength(1); j++)
            {
                row[j] = sum == 0 ? 0 : glcmMatrix[i, j] / sum;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    private double CalculateSumOfGlcm(int[,] glcmMatrix)
    {
        double sum = 0;
        for (int i = 0; i < glcmMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < glcmMatrix.GetLength(1); j++)
            {
                sum += glcmMatrix[i, j];
            }
        }

        return sum;
    }

    private IEnumerable<ImageTabControl> GetTabControls(DependencyObject parent)
    {
        var tabControls = new List<ImageTabControl>();
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ImageTabControl tabControl)
            {
                tabControls.Add(tabControl);
            }
            else if (child is DependencyObject dependencyObject)
            {
                tabControls.AddRange(GetTabControls(dependencyObject));
            }
        }

        return tabControls;
    }
}