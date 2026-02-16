using System.Windows.Controls;
using Axiom.UI.Themes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Axiom.UI.Editor;

public static class EditorConfigurator
{
    public static void Configure(TextEditor textEditor)
    {
        textEditor.Options.ConvertTabsToSpaces = true;
        textEditor.Options.IndentationSize = 4;
        textEditor.Options.EnableHyperlinks = false;
        textEditor.Options.HighlightCurrentLine = true;
        textEditor.Options.AllowScrollBelowDocument = false;
        textEditor.Options.ShowSpaces = false;

        // Font must be present inside Fonts.SystemFontFamilies
        textEditor.FontFamily = Stylesheet.FontFamily;
        textEditor.FontSize = Stylesheet.FontSize;

        textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

        textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Python");
    }
}