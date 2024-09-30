using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Imager.Controls;
using ModernWpf.Controls;
using GridView = ModernWpf.Controls.GridView;

namespace Imager.Windows;

public partial class TexturingWindow : Window
{
    private readonly ImageTabControl[] _tabControls;
    
    public TexturingWindow()
    {
        InitializeComponent();
        _tabControls = GetTabControls(MainGrid).ToArray();
    }

    private void CalculateCoOccurenceMatrices_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var imageTabControl in _tabControls)
        {
            
        }
    }

    private void NormalizeMatrices_OnClick(object sender, RoutedEventArgs e)
    {
        
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