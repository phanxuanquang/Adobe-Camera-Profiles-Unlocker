namespace Unlocker.Presentation.Ports;

public interface IDialogService
{
    /// <param name="confirmText">Verb label, e.g. "Remove 3". Cancel is always shown and is the default.</param>
    /// <returns>True when the user confirmed.</returns>
    Task<bool> ConfirmAsync(string title, string body, string confirmText);

    /// <param name="primaryText">Optional follow-up, e.g. "Buy me a coffee". Close is always shown.</param>
    /// <returns>True when the user chose the primary button.</returns>
    Task<bool> InformAsync(string title, string body, string primaryText);
}
