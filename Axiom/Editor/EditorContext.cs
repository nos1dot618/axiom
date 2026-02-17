using ICSharpCode.AvalonEdit;

namespace Axiom.Editor;

public static class EditorContext
{
    private static TextEditor? _editor;

    public static TextEditor? GetEditor() => _editor;
    public static void SetEditor(TextEditor editor) => _editor = editor;
}