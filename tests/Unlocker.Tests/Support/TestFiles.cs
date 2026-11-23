namespace Unlocker.Tests.Support;

internal static class TestFiles
{
    public static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
}
