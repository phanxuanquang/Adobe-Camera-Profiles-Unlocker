using System.Text.Json.Serialization;

namespace Unlocker.Infrastructure.Updates;

internal sealed record GitHubAsset([property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl);
