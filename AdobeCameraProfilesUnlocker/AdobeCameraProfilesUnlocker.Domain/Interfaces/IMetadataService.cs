namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IMetadataService
{
    Task EnsureDatasourceUpToDateAsync();
}