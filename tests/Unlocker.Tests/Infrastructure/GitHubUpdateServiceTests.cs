using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Infrastructure.Updates;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class GitHubUpdateServiceTests
{
    private const string Release = """
        {"tag_name":"v3.1.0","body":"notes","html_url":"https://github.com/x/y/releases/tag/v3.1.0",
         "published_at":"2026-01-01T00:00:00Z","assets":[{"browser_download_url":"https://github.com/x/y/a.zip"}]}
        """;

    private static GitHubUpdateService Updates(HttpStatusCode status, string body)
        => new(new HttpClient(new StubHttpHandler(status, body)) { BaseAddress = new Uri("https://api.github.com/") }, NullLogger<GitHubUpdateService>.Instance);

    [TestMethod]
    public async Task Returns_newer_release()
    {
        var info = await Updates(HttpStatusCode.OK, Release).GetNewerAsync(new Version(3, 0, 0));

        Assert.IsNotNull(info);
        Assert.AreEqual(new Version(3, 1, 0), info.Version);
        Assert.AreEqual("https://github.com/x/y/releases/tag/v3.1.0", info.PageUrl.AbsoluteUri);
    }

    [TestMethod]
    public async Task Returns_null_when_not_newer()
    {
        Assert.IsNull(await Updates(HttpStatusCode.OK, Release).GetNewerAsync(new Version(3, 1, 0)));
        Assert.IsNull(await Updates(HttpStatusCode.OK, Release.Replace("v3.1.0\"", "latest\"")).GetNewerAsync(new Version(1, 0)));
    }

    [TestMethod]
    public async Task Throws_on_failed_check()
    {
        await Assert.ThrowsAsync<HttpRequestException>(() => Updates(HttpStatusCode.InternalServerError, "").GetNewerAsync(new Version(1, 0)));
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => Updates(HttpStatusCode.OK, "not json").GetNewerAsync(new Version(1, 0)));
    }
}
