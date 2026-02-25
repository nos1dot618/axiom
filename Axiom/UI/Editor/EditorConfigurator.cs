using System.Windows.Controls;
using System.Windows.Media;
using Axiom.Editor;
using Axiom.UI.Themes;
using ICSharpCode.AvalonEdit;

namespace Axiom.UI.Editor;

public static class EditorConfigurator
{
    public static void Configure(TextEditor textEditor)
    {
        var settings = ServicesRegistry.SettingsService.CurrentSettings;

        textEditor.Options.ConvertTabsToSpaces = settings.Editor.ConvertTabsToSpaces;
        textEditor.Options.IndentationSize = settings.Editor.IndentationSize;
        textEditor.Options.EnableHyperlinks = settings.Editor.EnableHyperlinks;
        textEditor.Options.HighlightCurrentLine = settings.Editor.HighlightCurrentLine;
        textEditor.Options.AllowScrollBelowDocument = settings.Editor.AllowScrollBelowDocument;
        textEditor.Options.ShowSpaces = settings.Editor.ShowSpaces;

        // Font must be present inside Fonts.SystemFontFamilies
        textEditor.FontFamily = new FontFamily(settings.Editor.FontFamily);
        textEditor.FontSize = settings.Editor.FontSize;

        textEditor.VerticalScrollBarVisibility = ParseScrollBarVisibility(settings.Editor.VerticalScrollBarVisibility);
        textEditor.HorizontalScrollBarVisibility =
            ParseScrollBarVisibility(settings.Editor.HorizontalScrollBarVisibility);

        ServicesRegistry.ThemeService.SetTheme(settings.Editor.Theme);
        ThemeApplicator.Apply();
    }

    private static ScrollBarVisibility ParseScrollBarVisibility(bool visible)
    {
        return visible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
    }
}