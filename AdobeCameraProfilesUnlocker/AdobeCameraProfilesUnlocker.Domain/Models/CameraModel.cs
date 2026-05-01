using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Domain.Models;

public sealed record CameraModel
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public required string CodeName { get; init; }

    [Required]
    [ForeignKey(nameof(BrandId))]
    public Guid BrandId { get; set; }

    public CameraBrand Brand { get; set; } = default!;
    public ICollection<CameraProfile> Profiles { get; set; } = [];
}