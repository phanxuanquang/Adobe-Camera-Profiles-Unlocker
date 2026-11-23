namespace Unlocker.WinUI.Controls;

/// <summary>One brand in the grouped list. CollectionViewSource needs the group to be the item list itself.</summary>
public sealed partial class CameraPickerGroup(string key, IEnumerable<string> models) : List<string>(models)
{
    public string Key { get; } = key;
}
