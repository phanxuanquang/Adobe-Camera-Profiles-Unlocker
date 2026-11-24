using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Infrastructure.Storage;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class JsonSettingsTests : TempFolderTest
{
    [TestMethod]
    public void Locked_settings_file_starts_with_defaults()
    {
        Directory.CreateDirectory(Paths.AppDataDir);
        File.WriteAllText(Paths.SettingsFile, """{"TargetCamera":"Nikon Z f"}""");
        using var locked = new FileStream(Paths.SettingsFile, FileMode.Open, FileAccess.Read, FileShare.None);

        var settings = new JsonSettings(Paths, NullLogger<JsonSettings>.Instance);

        Assert.IsNull(settings.TargetCamera);
    }

    [TestMethod]
    public async Task Cancelled_save_keeps_previous_settings()
    {
        var settings = new JsonSettings(Paths, NullLogger<JsonSettings>.Instance) { TargetCamera = "Sony ILCE-7M4" };
        await settings.SaveAsync();

        settings.TargetCamera = "Canon EOS R5";
        await Assert.ThrowsAsync<OperationCanceledException>(() => settings.SaveAsync(new CancellationToken(canceled: true)));

        Assert.AreEqual("Sony ILCE-7M4", new JsonSettings(Paths, NullLogger<JsonSettings>.Instance).TargetCamera);
        CollectionAssert.AreEqual(new[] { Paths.SettingsFile }, Directory.GetFiles(Paths.AppDataDir));
    }
}
