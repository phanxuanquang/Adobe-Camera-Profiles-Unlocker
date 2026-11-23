namespace Unlocker.Presentation.Common;

public static class Text
{
    /// <summary>"1 camera profile", "3 camera profiles".</summary>
    public static string Profiles(int n) => n == 1 ? "1 camera profile" : $"{n} camera profiles";
}
