using Unlocker.Presentation.Ports;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeDialogs : IDialogService
{
    public bool Answer { get; set; } = true;
    public string? LastTitle { get; private set; }
    public string? LastConfirm { get; private set; }

    public Task<bool> ConfirmAsync(string title, string body, string confirmText)
    {
        LastTitle = title;
        LastConfirm = confirmText;
        return Task.FromResult(Answer);
    }
}
