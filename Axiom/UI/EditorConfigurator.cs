using System.Windows.Controls;
using ICSharpCode.AvalonEdit;

namespace Axiom.UI;

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

        textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
    }
}