namespace Unlocker.Presentation.Ports;

public interface IDialogService
{
    /// <param name="confirmText">Verb label, e.g. "Remove 3". Cancel is always shown and is the default.</param>
    /// <returns>True when the user confirmed.</returns>
    Task<bool> ConfirmAsync(string title, string body, string confirmText);
}
