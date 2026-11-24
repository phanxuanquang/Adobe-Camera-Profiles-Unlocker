namespace Unlocker.Core.Models;

/// <param name="Family">What the brand calls its camera profiles, e.g. "Film Simulations".</param>
public sealed record Brand(string Name, string Family, IReadOnlyList<CameraModel> Models)
{
    /// <summary>Default source model: the one with the most camera profiles.</summary>
    public CameraModel BestModel => Models.MaxBy(m => m.Profiles.Count)!;
}
