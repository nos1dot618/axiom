using System.Windows;

namespace Axiom;

public partial class MainWindow : Window
{
    private const string TextBuffer = "Axiom Editor\n\nStart typing...";

    public MainWindow()
    {
        InitializeComponent();

        Editor.Text = TextBuffer;
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.IndentationSize = 4;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.HighlightCurrentLine = true;
        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowSpaces = false;
    }
}