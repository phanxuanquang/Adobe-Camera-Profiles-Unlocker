namespace Unlocker.Presentation.Unlock;

/// <summary>Search results for one brand, e.g. "Fujifilm Film Simulations".</summary>
public sealed record CameraProfileGroup(string Title, IReadOnlyList<CameraProfileItemViewModel> Items);
