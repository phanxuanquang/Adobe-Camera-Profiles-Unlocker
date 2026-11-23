using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

public interface IEnvironmentProbe
{
    IReadOnlyList<EnvironmentIssue> Check();
}
