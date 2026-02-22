using ICSharpCode.AvalonEdit;

namespace Axiom.Editor;

// TODO: Merge this into EditorService.
public static class EditorContext
{
    private static TextEditor? _editor;

    public static TextEditor GetEditor()
    {
        return _editor ?? throw new InvalidOperationException("Editor has not been initialized.");
    }

    public static void SetEditor(TextEditor editor)
    {
        _editor = editor;
    }
}