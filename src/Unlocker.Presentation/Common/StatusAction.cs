using CommunityToolkit.Mvvm.Input;

namespace Unlocker.Presentation.Common;

/// <summary>Button inside a status message, e.g. "Undo".</summary>
public sealed record StatusAction(string Label, IRelayCommand Command);
