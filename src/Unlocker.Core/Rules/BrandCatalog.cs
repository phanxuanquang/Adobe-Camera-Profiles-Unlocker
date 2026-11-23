namespace Unlocker.Core.Rules;

public static class BrandCatalog
{
    private static readonly Dictionary<string, string> Families = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fujifilm"] = "Film Simulations",
        ["Canon"] = "Picture Styles",
        ["Nikon"] = "Picture Controls",
        ["Sony"] = "Creative Looks",
        ["Panasonic"] = "Photo Styles",
        ["Leica"] = "Film Styles",
        ["Olympus"] = "Picture Modes",
        ["OM"] = "Picture Modes",
        ["Pentax"] = "Custom Images",
        ["Ricoh"] = "Custom Images",
        ["Sigma"] = "Color Modes",
    };

    public static string FamilyOf(string brand) => Families.GetValueOrDefault(brand, "Camera Profiles");

    // ponytail: first word of the model ("OM Digital Solutions OM-1" -> "OM"). Add an alias map if a brand groups wrong.
    public static string BrandOf(string model) => model.Split(' ', 2)[0];
}
