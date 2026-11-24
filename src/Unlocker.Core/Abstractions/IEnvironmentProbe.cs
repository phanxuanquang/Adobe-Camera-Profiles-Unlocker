using Unlocker.Core.Enums;

namespace Unlocker.Core.Abstractions;

public interface IEnvironmentProbe
{
    IReadOnlyList<EnvironmentIssue> Check();
}
