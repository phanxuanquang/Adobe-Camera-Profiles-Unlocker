using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Serialization;

namespace Unlocker.Infrastructure.Updates;

internal sealed class GitHubUpdateService(HttpClient http, ILogger<GitHubUpdateService> logger) : IUpdateService
{
    private const string LatestRelease = "repos/phanxuanquang/Adobe-Camera-Profiles-Unlocker/releases/latest";

    public async Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default)
    {
        var release = await http.GetFromJsonAsync(LatestRelease, UnlockerJsonContext.Default.GitHubRelease, ct);
        if (release is null || ParseVersion(release.TagName) is not { } version || version <= current) return null;
        // The page opens in the browser: only trust a GitHub https link.
        if (!Uri.TryCreate(release.HtmlUrl, UriKind.Absolute, out var page) || page.Scheme != Uri.UriSchemeHttps || page.Host != "github.com")
        {
            logger.LogWarning("Ignoring release {Version} with untrusted page URL '{Url}'", version, release.HtmlUrl);
            return null;
        }
        logger.LogInformation("Newer version {Version} available (current {Current}): {Url}", version, current, page);
        return new ReleaseInfo(version, page);
    }

    /// <summary>"v3.1.0" and "Neo 2.0" parse; anything without a version gives null.</summary>
    private static Version? ParseVersion(string tag)
    {
        var start = tag.IndexOfAny("0123456789".ToCharArray());
        return start >= 0 && Version.TryParse(tag[start..], out var v) ? v : null;
    }
}
