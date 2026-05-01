namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IDcpToolService
{
    Task EnsureResourceInitializedAsync();
    Task DecompileDcpIntoXmlAsync(IEnumerable<Guid> cameraProfileIds);
    Task CompileXmlIntoDcpAsync(IEnumerable<Guid> cameraProfileIds);
}