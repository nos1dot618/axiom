using ICSharpCode.AvalonEdit;

namespace Axiom.Editor;

public static class EditorContext
{
    private static TextEditor? _editor;

    public static TextEditor GetEditor() =>
        _editor ?? throw new InvalidOperationException("Editor has not been initialized.");

    public static void SetEditor(TextEditor editor) => _editor = editor;
}