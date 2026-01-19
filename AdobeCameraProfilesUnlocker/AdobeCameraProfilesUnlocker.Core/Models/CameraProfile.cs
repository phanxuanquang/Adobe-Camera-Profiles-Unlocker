using AdobeCameraProfilesUnlocker.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeCameraProfilesUnlocker.Core.Models
{
    public class CameraProfile
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required CameraProfileExtension ExtensionId { get; set; }
        public required string FilePath { get; set; }

        [ForeignKey(nameof(CameraId))]
        public int CameraId { get; set; }

        public virtual Camera Camera { get; set; }
    }
}
