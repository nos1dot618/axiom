// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Axiom.Core.Settings.Sections;

public class EditorSection
{
    public string FontFamily { get; set; } = "Consolas";
    public int FontSize { get; set; } = 24;
    public bool ConvertTabsToSpaces { get; set; } = true;
    public int IndentationSize { get; set; } = 4;
    public bool EnableHyperlinks { get; set; } = false;
    public bool HighlightCurrentLine { get; set; } = true;
    public bool AllowScrollBelowDocument { get; set; } = false;
    public bool ShowSpaces { get; set; } = false;
    public bool VerticalScrollBarVisibility { get; set; } = false;
    public bool HorizontalScrollBarVisibility { get; set; } = false;

    // TODO: Editor should have default theme instead of relying on default values of EditorTheme class.
    public string Theme { get; set; } = "Unnamed";
}