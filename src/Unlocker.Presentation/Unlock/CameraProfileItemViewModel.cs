using CommunityToolkit.Mvvm.ComponentModel;
using Unlocker.Core.Models;

namespace Unlocker.Presentation.Unlock;

/// <summary>One camera profile tile. The same instance is the tray chip when selected.</summary>
public sealed partial class CameraProfileItemViewModel(Brand brand, CameraModel source, CameraProfile profile) : ObservableObject
{
    public Brand Brand { get; } = brand;
    public CameraModel Source { get; } = source;
    public CameraProfile Profile { get; } = profile;
    public string Name => Profile.Name;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>Already installed for the current target camera.</summary>
    [ObservableProperty]
    public partial bool IsInstalled { get; set; }
}
