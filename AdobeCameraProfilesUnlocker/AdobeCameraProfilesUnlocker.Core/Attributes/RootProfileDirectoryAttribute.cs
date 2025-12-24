namespace AdobeCameraProfilesUnlocker.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RootProfileDirectoryAttribute(string directory) : Attribute
    {
        public string Directory { get; } = directory;
    }
}
