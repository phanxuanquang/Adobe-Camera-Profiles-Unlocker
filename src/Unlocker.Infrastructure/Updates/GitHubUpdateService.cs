using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
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
        logger.LogInformation("Newer version {Version} available (current {Current}): {Url}", version, current, release.HtmlUrl);
        return new ReleaseInfo(version, new Uri(release.HtmlUrl));
    }

    /// <summary>"v3.1.0" and "Neo 2.0" parse; anything without a version gives null.</summary>
    private static Version? ParseVersion(string tag)
    {
        var start = tag.IndexOfAny("0123456789".ToCharArray());
        return start >= 0 && Version.TryParse(tag[start..], out var v) ? v : null;
    }
}
