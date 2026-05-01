using System.ComponentModel.DataAnnotations;

namespace AdobeCameraProfilesUnlocker.Domain.Models;

public sealed record CameraBrand
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Name { get; init; }
    public ICollection<CameraModel> CameraModels { get; set; } = [];
    public ICollection<CameraProfile> Profiles { get; set; } = [];
}