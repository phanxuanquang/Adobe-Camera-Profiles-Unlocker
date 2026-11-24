using System.Text.Json.Serialization;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Storage;
using Unlocker.Infrastructure.Updates;

namespace Unlocker.Infrastructure.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsData))]
[JsonSerializable(typeof(List<InstalledCameraProfile>))]
[JsonSerializable(typeof(GitHubRelease))]
internal sealed partial class UnlockerJsonContext : JsonSerializerContext;
