using CommunityToolkit.Mvvm.ComponentModel;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;

namespace Unlocker.Presentation.Unlock;

public sealed partial class BrandItemViewModel(Brand brand) : ObservableObject
{
    public Brand Brand { get; } = brand;
    public string Name => Brand.Name;
    public string Family => Brand.Family;

    /// <summary>The model camera profiles are copied from ("Copied from").</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourceLabel))]
    public partial CameraModel SourceModel { get; set; } = brand.BestModel;

    /// <summary>"Copied from" choices. The best model is marked when there is a choice.</summary>
    public IReadOnlyList<string> ModelLabels { get; } = brand.Models
        .Select(m => m == brand.BestModel && brand.Models.Count > 1 ? $"{m.Name} (most camera profiles)" : m.Name)
        .ToList();

    /// <summary>
    /// Label of <see cref="SourceModel"/>. A label not in <see cref="ModelLabels"/> is ignored: while the brand changes,
    /// the ComboBox pushes null or the previous brand's label. An index would be valid for both brands.
    /// </summary>
    public string? SourceLabel
    {
        get => ModelLabels[Brand.Models.ToList().IndexOf(SourceModel)];
        set
        {
            var i = value is null ? -1 : ModelLabels.ToList().IndexOf(value);
            if (i >= 0) SourceModel = Brand.Models[i];
        }
    }

    /// <summary>Camera profiles that match the current search, or all of them without a search.</summary>
    [ObservableProperty]
    public partial int VisibleCount { get; set; }

    /// <summary>Selected among the visible ones, so the count never reads "3/2".</summary>
    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial bool IsYourBrand { get; set; }

    /// <summary>Screen readers read ListView items through ToString; the count column is visual only.</summary>
    public override string ToString() => $"{Name}, {Family}, " + (SelectedCount > 0 ? $"{SelectedCount} selected of {Text.Profiles(VisibleCount)}" : Text.Profiles(VisibleCount));
}
