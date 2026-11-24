using Unlocker.Presentation.Ports;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeDialogs : IDialogService
{
    public bool Answer { get; set; } = true;
    public string? LastTitle { get; private set; }
    public string? LastBody { get; private set; }
    public string? LastConfirm { get; private set; }
    public Exception? FailWith { get; set; }

    public Task<bool> ConfirmAsync(string title, string body, string confirmText) => Show(title, body, confirmText);

    public Task<bool> InformAsync(string title, string body, string primaryText) => Show(title, body, primaryText);

    private Task<bool> Show(string title, string body, string button)
    {
        if (FailWith is { } ex) throw ex;
        (LastTitle, LastBody, LastConfirm) = (title, body, button);
        return Task.FromResult(Answer);
    }
}
