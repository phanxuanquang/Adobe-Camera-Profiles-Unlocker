namespace Unlocker.Presentation.Common;

/// <summary>UI-neutral InfoBar content. A null message on a view model means the InfoBar is closed.</summary>
public sealed record StatusMessage(StatusSeverity Severity, string Title, string Message, IReadOnlyList<StatusAction>? Actions = null);
