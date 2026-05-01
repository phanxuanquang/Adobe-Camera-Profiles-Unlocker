using AdobeCameraProfilesUnlocker.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Domain.Models;

public sealed record CameraProfile
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required CameraProfileType FileType { get; init; }
    public required string FilePath { get; init; }

    [ForeignKey(nameof(CameraBrandId))]
    public Guid CameraBrandId { get; set; }
    public CameraBrand Brand { get; set; } = default!;
}