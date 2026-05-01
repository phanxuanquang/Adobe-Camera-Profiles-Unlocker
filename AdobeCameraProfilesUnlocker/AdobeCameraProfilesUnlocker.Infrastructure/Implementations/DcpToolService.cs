using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class DcpToolService(IOptionsSnapshot<DcpToolOptions> options, ILoggerFactory? loggerFactory = null) : IDcpToolService
{
    private readonly ILogger<DcpToolService> _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DcpToolService>();
    private readonly DcpToolOptions _options = options.Value;

    public async Task CompileXmlIntoDcpAsync(IEnumerable<Guid> cameraProfileIds)
    {
        _logger.LogTrace("Compiling XML files into DCP for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));
        throw new NotImplementedException();
    }

    public async Task DecompileDcpIntoXmlAsync(IEnumerable<Guid> cameraProfileIds)
    {
        _logger.LogTrace("Decompiling DCP files into XML for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));
        throw new NotImplementedException();
    }

    public async Task EnsureResourceInitializedAsync()
    {
        throw new NotImplementedException();
    }
}