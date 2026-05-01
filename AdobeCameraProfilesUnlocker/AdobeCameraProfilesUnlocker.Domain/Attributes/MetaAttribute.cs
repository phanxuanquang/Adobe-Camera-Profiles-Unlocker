namespace AdobeCameraProfilesUnlocker.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MetaAttribute(string fileExtension) : Attribute
{
    public string FileExtension { get; } = fileExtension;
}