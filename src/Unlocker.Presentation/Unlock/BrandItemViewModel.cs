using CommunityToolkit.Mvvm.ComponentModel;
using Unlocker.Core.Models;

namespace Unlocker.Presentation.Unlock;

public sealed partial class BrandItemViewModel(Brand brand) : ObservableObject
{
    public Brand Brand { get; } = brand;
    public string Name => Brand.Name;
    public string Family => Brand.Family;

    /// <summary>The model camera profiles are copied from ("Copied from").</summary>
    [ObservableProperty]
    public partial CameraModel SourceModel { get; set; } = brand.BestModel;

    /// <summary>Camera profiles that match the current search, or all of them without a search.</summary>
    [ObservableProperty]
    public partial int VisibleCount { get; set; }

    /// <summary>Selected among the visible ones, so the count never reads "3/2".</summary>
    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial bool IsYourBrand { get; set; }

    /// <summary>Screen readers read ListView items through ToString.</summary>
    public override string ToString() => $"{Name}, {Family}";
}
