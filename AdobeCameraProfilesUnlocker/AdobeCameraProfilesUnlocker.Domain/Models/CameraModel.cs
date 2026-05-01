using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Domain.Models;

public sealed record CameraModel
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string CodeName { get; init; }

    [ForeignKey(nameof(BrandId))]
    public Guid BrandId { get; set; }
    public CameraBrand Brand { get; set; } = default!;
}