// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using Axiom.Core.Settings.Sections.Editor.Appearance;

namespace Axiom.Core.Settings.Sections.Editor;

public class EditorSection
{
    public string FontFamily { get; set; } = "Consolas";
    public double FontSize { get; set; } = 24.0;
    public bool ConvertTabsToSpaces { get; set; } = true;
    public int IndentationSize { get; set; } = 4;
    public bool EnableHyperlinks { get; set; } = false;
    public bool HighlightCurrentLine { get; set; } = true;
    public bool AllowScrollBelowDocument { get; set; } = false;
    public bool ShowSpaces { get; set; } = false;
    public bool VerticalScrollBarVisibility { get; set; } = true;
    public bool HorizontalScrollBarVisibility { get; set; } = false;

    public AppearanceSection Appearance { get; set; } = new();

    // TODO: Editor should have default theme instead of relying on default values of EditorTheme class.
    public string Theme { get; set; } = "Unnamed";
}