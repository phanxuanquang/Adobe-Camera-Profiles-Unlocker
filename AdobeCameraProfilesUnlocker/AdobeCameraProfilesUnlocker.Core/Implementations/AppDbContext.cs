using AdobeCameraProfilesUnlocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AdobeCameraProfilesUnlocker.Core.Implementations
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<CameraBrand> CameraBrands { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraProfile> CameraProfiles { get; set; }
    }
}
