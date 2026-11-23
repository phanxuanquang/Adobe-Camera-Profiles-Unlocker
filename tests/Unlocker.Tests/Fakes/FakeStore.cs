using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeStore : IInstalledStore
{
    private readonly List<InstalledCameraProfile> _items = [];

    public Exception? FailRemoveWith { get; set; }

    public IReadOnlyList<InstalledCameraProfile> All => _items;

    public IReadOnlyList<string> Folders { get; } = [@"C:\out\dcp", @"C:\out\xmp"];

    public event EventHandler? Changed;

    public Task LoadAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default)
    {
        _items.Add(item);
        Changed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default)
    {
        if (FailRemoveWith is { } ex) throw ex;
        foreach (var i in items.ToList()) _items.Remove(i);
        Changed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task RestoreAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default)
    {
        _items.AddRange(items);
        Changed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
