using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using AdobeCameraProfilesUnlocker.Infrastructure.Implementations;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AdobeCameraProfilesUnlocker.ConsoleApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Trace);
        });

        var logger = LoggerFactory
            .Create(config =>
            {
                config.AddConsole();
                config.SetMinimumLevel(LogLevel.Trace);
            })
            .CreateLogger<Program>();

        builder.Services.AddDbContextFactory<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!.Replace("%USERNAME%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var dbFilePathInConnectionString = new SqliteConnectionStringBuilder(connectionString).DataSource;
            var dbDirectoryPath = Path.GetDirectoryName(dbFilePathInConnectionString)!;
            if (!Directory.Exists(dbDirectoryPath))
            {
                logger.LogWarning("Database directory does not exist. Creating directory at path: {dbDirectoryPath}", dbDirectoryPath);
                Directory.CreateDirectory(dbDirectoryPath);
            }
            options.UseSqlite(connectionString);
        });

        builder.Services.Configure<ResourceOptions>(builder.Configuration.GetSection(nameof(ResourceOptions)));
        builder.Services.AddScoped<IResourceService, LocalResourceService>();
        builder.Services.AddScoped<IBrandService, BrandService>();
        builder.Services.AddScoped<ICameraProfileService, CameraProfileService>();

        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Start();
    }
}