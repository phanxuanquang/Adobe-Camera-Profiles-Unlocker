using System.Text.Json.Serialization;

namespace Unlocker.Infrastructure.Updates;

internal sealed record GitHubRelease(
    [property: JsonPropertyName("tag_name")] string TagName,
    [property: JsonPropertyName("html_url")] string HtmlUrl);
