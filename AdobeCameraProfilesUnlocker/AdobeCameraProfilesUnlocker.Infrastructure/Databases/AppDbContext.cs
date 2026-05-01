using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases.Models;
using Microsoft.EntityFrameworkCore;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Databases;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CameraBrand> Brands { get; set; }
    public DbSet<CameraModel> Cameras { get; set; }
    public DbSet<CameraProfile> Profiles { get; set; }
    public DbSet<Meta> Metas { get; set; }
}