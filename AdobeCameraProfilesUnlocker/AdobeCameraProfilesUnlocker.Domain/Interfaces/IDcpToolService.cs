namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IDcpToolService
{
    /// <summary>
    /// Ensures that the underlying DcpTool resource is initialized asynchronously if it has not already been initialized.
    /// </summary>
    /// <remarks>If the resource is already initialized, this method returns a completed task. Multiple
    /// concurrent calls are safe; initialization occurs only once.</remarks>
    /// <returns>A task that represents the asynchronous initialization operation. The task completes when the resource is fully
    /// initialized.</returns>
    Task EnsureResourceInitializedAsync();

    /// <summary>
    /// Decompiles the specified DCP files into XML format. The method takes a collection of camera profile IDs, which correspond to the DCP files to be decompiled. 
    /// Optionally, a target directory can be specified for the output XML files; if not provided, the Temp directory will be used. The method performs the decompilation asynchronously and ensures that the resulting XML files are properly saved in the target location.
    /// </summary>
    /// <param name="cameraProfileIds">A collection of camera profile IDs corresponding to the DCP files to be decompiled.</param>
    /// <param name="targetDirectory">The directory where the output XML files will be saved. If not provided, the Temp directory will be used.</param>
    /// <returns>A task that represents the asynchronous decompilation operation. The task completes when all specified DCP files have been decompiled into XML.</returns>
    Task DecompileDcpIntoXmlAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null);

    /// <summary>
    /// Compiles the specified XML files into DCP format. The method takes a collection of camera profile IDs, which correspond to the XML files to be compiled. 
    /// Optionally, a target directory can be specified for the output DCP files; if not provided, the Temp directory will be used. The method performs the compilation asynchronously and ensures that the resulting DCP files are properly saved in the target location.
    /// </summary>
    /// <param name="cameraProfileIds">A collection of camera profile IDs corresponding to the XML files to be compiled.</param>
    /// <param name="targetDirectory">The directory where the output DCP files will be saved. If not provided, the Temp directory will be used.</param>
    /// <returns>A task that represents the asynchronous compilation operation. The task completes when all specified XML files have been compiled into DCP.</returns>
    Task CompileXmlIntoDcpAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null);
}