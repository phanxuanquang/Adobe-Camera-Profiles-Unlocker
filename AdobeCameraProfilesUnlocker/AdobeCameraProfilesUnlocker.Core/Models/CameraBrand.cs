using System.ComponentModel.DataAnnotations;

namespace AdobeCameraProfilesUnlocker.Core.Models
{
    public class CameraBrand
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
