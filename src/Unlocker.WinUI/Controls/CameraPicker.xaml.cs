using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Core.Models;

namespace Unlocker.WinUI.Controls;

/// <summary>Camera models grouped by brand, filtered by text and brand. Raises Picked with the model name.</summary>
public sealed partial class CameraPicker : UserControl
{
    private const string AllBrands = "All brands";

    private IReadOnlyList<Brand> _catalog = [];
    private string? _current;

    public CameraPicker() => InitializeComponent();

    public event EventHandler<string>? Picked;

    public void Show(IReadOnlyList<Brand> catalog, string? current)
    {
        _catalog = catalog;
        _current = current;
        Filter.Text = "";
        BrandFilter.ItemsSource = catalog.Select(b => b.Name).Prepend(AllBrands).ToList();
        BrandFilter.SelectedIndex = 0;
        Draw();
    }

    public void FocusSearch() => Filter.Focus(FocusState.Programmatic);

    private void Draw()
    {
        var q = Filter.Text.Trim();
        var brand = BrandFilter.SelectedIndex > 0 ? (string)BrandFilter.SelectedItem : null;
        var groups = _catalog
            .Where(b => brand is null || b.Name == brand)
            .Select(b => new CameraPickerGroup(b.Name, b.Models
                .Where(m => m.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.Name)))
            .Where(g => g.Count > 0)
            .ToList();
        Groups.Source = groups;
        List.SelectedItem = _current;
        if (_current is not null) List.ScrollIntoView(_current);
        Empty.Text = $"No camera matches \"{q}\". Check the model name on the camera's label.";
        Empty.Visibility = groups.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Filter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e) => Draw();

    private void BrandFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => Draw();

    private void List_ItemClick(object sender, ItemClickEventArgs e) => Picked?.Invoke(this, (string)e.ClickedItem);
}
