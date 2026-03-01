using Axiom.UI.Editor;

namespace Axiom.App;

public partial class ReadOnlyWindow
{
    public ReadOnlyWindow(string title, string contents)
    {
        InitializeComponent();

        Title = title;
        Editor.Text = contents;
        EditorUiController.Configure(Editor, false);
    }

    public void AppendText(string text)
    {
        Editor.AppendText(text);
        Editor.ScrollToEnd();
    }
}