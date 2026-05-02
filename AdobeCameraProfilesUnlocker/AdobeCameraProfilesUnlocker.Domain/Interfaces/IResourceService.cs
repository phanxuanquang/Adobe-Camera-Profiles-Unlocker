namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IResourceService
{
    Task EnsureDatasourceUpToDateAsync();
    Task ForceUpdateDatasourceAsync();
}