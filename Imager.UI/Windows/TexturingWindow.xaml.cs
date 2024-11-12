using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageChannelSplitter.Implementations;
using Imager.Controls;
using Imager.Converters;
using Imager.Processors.Calculators;
using Imager.Processors.Processors;
using Imager.Utils;

namespace Imager.Windows;

public partial class TexturingWindow : Window
{
    private readonly string[] _cfs = ["Energy", "Contrast", "Homogeinity", "R"];
    private const int DecimalPlaces = 5;

    private readonly (NumberedImageTabControl imageTabControl, int[,]? glcm, int gradationsCount)[] _tabControls;
    private readonly Normalizer _normalizer;
    private SignsCoefs? _mainImageSignsCoeffs;

    public TexturingWindow()
    {
        InitializeComponent();
        _normalizer = new Normalizer();

        _tabControls = GetTabControls(MainGrid)
            .Select(tab => (tab, glcm: (int[,]?)null, 0))
            .ToArray();

        InitCfsDataGrid();
    }

    private void InitCfsDataGrid()
    {
        var dataTable = new DataTable();

        foreach (var coefficient in _cfs)
        {
            dataTable.Columns.Add(coefficient);
        }

        for (var i = 0; i < _tabControls.Length; i++)
        {
            dataTable.Rows.Add();
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

    private void CalculateAndDisplayGlcm(
        (NumberedImageTabControl imageTabControl, int[,]? glcm, int gradationsCount) imageTabControl,
        int distanceNum, int angle)
    {
        if (imageTabControl.imageTabControl.ImageChannelSplitter?.ChannelsMatrices.HalftoneMatrix is null) return;

        var halftoneMatrix = imageTabControl.imageTabControl.ImageChannelSplitter.ChannelsMatrices.HalftoneMatrix;
        var glcm = GlcmCalculator.CalculateGlcm(halftoneMatrix, distanceNum, angle, out var gradationsCount);

        var dataTable = DataTableFromMatrixCreator.ConvertMatrixToDataTable(glcm);
        imageTabControl.imageTabControl.CoOcurrenceMatrix.ItemsSource = dataTable.DefaultView;

        var index = Array.FindIndex(_tabControls, t => t.imageTabControl == imageTabControl.imageTabControl);

        if (index != -1)
        {
            _tabControls[index] = (imageTabControl.imageTabControl, glcm, gradationsCount);
        }
    }

    private void NormalizeMatrices_OnClick(object sender, RoutedEventArgs e)
    {
        _mainImageSignsCoeffs = null;
        for (var index = 0; index < _tabControls.Length; index++)
        {
            var tabControlTuple = _tabControls[index];
            NormalizeAndDisplayGlcmWithCfs(index, tabControlTuple.imageTabControl, tabControlTuple.glcm,
                tabControlTuple.gradationsCount);
        }
    }

    private void NormalizeAndDisplayGlcmWithCfs(int index, NumberedImageTabControl imageTabControl, int[,]? glcmMatrix,
        int gradationsCount)
    {
        if (glcmMatrix is null) return;

        var normalizedMatrix = _normalizer.NormalizeMatrixBySum(glcmMatrix, DecimalPlaces);
        FillCfsDataTable(index, normalizedMatrix, gradationsCount);

        imageTabControl.NormalizedCoOcurrenceMatrix.ItemsSource =
            DataTableFromMatrixCreator.ConvertMatrixToDataTable(normalizedMatrix).DefaultView;
    }

    private void FillCfsDataTable(int index, double[,] normalizedMatrix, int gradationsCount)
    {
        var dataTable = (CfsDataGrid.ItemsSource as DataView)?.Table;

        if (dataTable is null) return;
        var row = dataTable.Rows[index];
        var thisSignsCoeff = new SignsCoefs();

        row[0] = thisSignsCoeff.Energy = SignsCalculator.CalculateEnergy(normalizedMatrix, gradationsCount);
        row[1] = thisSignsCoeff.Contrast =
            SignsCalculator.CalculateContrast(normalizedMatrix, gradationsCount);
        row[2] = thisSignsCoeff.Homogeinity =
            SignsCalculator.CalculateHomogenity(normalizedMatrix, gradationsCount);

        if (_mainImageSignsCoeffs.HasValue && index != 0)
        {
            row[3] = thisSignsCoeff.R =
                SignsCalculator.CalculateR(thisSignsCoeff, _mainImageSignsCoeffs.Value);
        }

        if (index == 0)
        {
            _mainImageSignsCoeffs = thisSignsCoeff;
        }
    }

    private IEnumerable<NumberedImageTabControl> GetTabControls(DependencyObject parent)
    {
        var tabControls = new List<NumberedImageTabControl>();
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is NumberedImageTabControl tabControl)
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