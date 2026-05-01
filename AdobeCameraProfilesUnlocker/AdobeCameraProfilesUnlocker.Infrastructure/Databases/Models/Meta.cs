namespace AdobeCameraProfilesUnlocker.Infrastructure.Databases.Models;

public class Meta
{
    public Guid Id { get; private set; }
    public DateTime LastUpdatedTime { get; set; } = DateTime.UtcNow;
}