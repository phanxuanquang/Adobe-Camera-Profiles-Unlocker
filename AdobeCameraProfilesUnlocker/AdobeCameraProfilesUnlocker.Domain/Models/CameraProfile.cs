using AdobeCameraProfilesUnlocker.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Domain.Models;

public sealed record CameraProfile
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public required string Name { get; init; }

    [Required]
    public required CameraProfileType FileType { get; init; }

    [Required]
    public required string FilePath { get; init; }

    [Required]
    [ForeignKey(nameof(CameraId))]
    public required Guid CameraId { get; set; }

    public CameraModel Camera { get; set; } = default!;
}