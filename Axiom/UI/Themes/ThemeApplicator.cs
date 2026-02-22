using System.Windows.Media;
using Axiom.Editor;
using Axiom.Editor.Lsp;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Axiom.UI.Themes;

public static class ThemeApplicator
{
    public static void Apply()
    {
        var editor = EditorContext.GetEditor();
        var theme = ServiceFactory.ThemeService.CurrentTheme;

        editor.Background = Brush(theme.Editor.Background);
        editor.Foreground = Brush(theme.Editor.Foreground);

        editor.TextArea.Caret.CaretBrush = Brush(theme.Editor.Caret);
        editor.TextArea.SelectionBrush = Brush(theme.Editor.Selection);
        editor.TextArea.SelectionForeground = Brush(theme.Editor.SelectionForeground);

        editor.ShowLineNumbers = true;
        editor.LineNumbersForeground = Brush(theme.Editor.LineNumbers);
    }

    public static void ApplySyntaxHighlighting()
    {
        var editor = EditorContext.GetEditor();
        var theme = ServiceFactory.ThemeService.CurrentTheme;

        var highlighting = HighlightingManager.Instance.HighlightingDefinitions.FirstOrDefault(h =>
            string.Equals(h.Name, LspSession.LanguageId, StringComparison.OrdinalIgnoreCase));

        Set("Keywords", theme.Syntax.Keyword);
        Set("String", theme.Syntax.String);
        Set("Comment", theme.Syntax.Comment);
        Set("NumberLiteral", theme.Syntax.Number);
        Set("MethodCall", theme.Syntax.Method);

        editor.SyntaxHighlighting = highlighting;
        return;

        void Set(string name, string hex)
        {
            var color = highlighting?.GetNamedColor(name);
            if (color != null)
                color.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }

    private static SolidColorBrush Brush(string hex)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }
}