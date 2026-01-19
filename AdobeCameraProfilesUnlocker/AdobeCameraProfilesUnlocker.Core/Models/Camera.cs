using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Core.Models
{
    public class Camera
    {
        [Key]
        public int Id { get; set; }
        public required string CodeName { get; set; }

        [ForeignKey(nameof(BrandId))]
        public required int BrandId { get; set; }

        public virtual ICollection<CameraProfile> CameraProfiles { get; set; } = [];
        public virtual CameraBrand CameraBrand { get; set; } = null!;
    }
}
