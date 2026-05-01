using System.Xml.Linq;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Helpers;

internal static class XmlHelper
{
    internal static void UpdateAttributes(string filePath, Dictionary<string, string> attributes)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("XML file not found.", filePath);

        var doc = XDocument.Load(filePath);
        bool hasChanged = false;

        foreach (var element in doc.Descendants())
        {
            foreach (var (attrName, newValue) in attributes)
            {
                var attr = element.Attribute(attrName);

                if (attr is null || attr.Value == newValue)
                    continue;

                attr.Value = newValue;
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            doc.Save(filePath);
        }
    }
}