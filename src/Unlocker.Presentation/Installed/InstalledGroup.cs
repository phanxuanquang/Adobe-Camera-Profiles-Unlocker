using Unlocker.Core.Models;

namespace Unlocker.Presentation.Installed;

public sealed record InstalledGroup(string TargetModel, IReadOnlyList<InstalledCameraProfile> Items);
