using System.Reflection;
using System.Windows.Media;
using System.Xml;
using Axiom.Editor;
using Axiom.Editor.Documents;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Axiom.UI.Themes;

public static class ThemeApplicator
{
    private static readonly (string Resource, string Name, string[] Extensions)[] CustomSyntaxHighlightings =
    [
        ("Axiom.Resources.SyntaxHighlighting.Go-Mode.xshd", "Go", [".go"])
    ];

    static ThemeApplicator()
    {
        RegisterSyntaxHighlightings();
    }

    public static void Apply()
    {
        var editor = EditorService.Editor;
        var theme = ServicesRegistry.ThemeService.CurrentTheme;

        editor.Background = Brush(theme.Editor.Background);
        editor.Foreground = Brush(theme.Editor.Foreground);

        editor.TextArea.Caret.CaretBrush = Brush(theme.Editor.Caret);
        editor.TextArea.SelectionBrush = Brush(theme.Editor.Selection);
        editor.TextArea.SelectionForeground = Brush(theme.Editor.SelectionForeground);

        editor.ShowLineNumbers = true;
        editor.LineNumbersForeground = Brush(theme.Editor.LineNumbers);

        ApplySyntaxHighlighting();
    }

    public static void ApplySyntaxHighlighting()
    {
        var editor = EditorService.Editor;
        var theme = ServicesRegistry.ThemeService.CurrentTheme;

        var highlighting = HighlightingManager.Instance.HighlightingDefinitions.FirstOrDefault(h =>
            string.Equals(h.Name, FileService.CurrentBuffer.LanguageId, StringComparison.OrdinalIgnoreCase));

        Set("Keywords", theme.Syntax.Keyword);
        Set("String", theme.Syntax.String);
        Set("Comment", theme.Syntax.Comment);
        Set("NumberLiteral", theme.Syntax.Number);
        Set("MethodCall", theme.Syntax.Method);
        Set("Type", theme.Syntax.Type);
        Set("Builtin", theme.Syntax.Builtin);

        editor.SyntaxHighlighting = highlighting;
        return;

        void Set(string name, string hex)
        {
            var color = highlighting?.GetNamedColor(name);
            if (color != null)
                color.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }

    public static void RemoveSyntaxHighlighting()
    {
        EditorService.Editor.SyntaxHighlighting = null;
    }

    private static void RegisterSyntaxHighlightings()
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var highlighting in CustomSyntaxHighlightings)
        {
            using var stream = assembly.GetManifestResourceStream(highlighting.Resource);
            if (stream == null) return;

            using var reader = new XmlTextReader(stream);
            var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            HighlightingManager.Instance.RegisterHighlighting(highlighting.Name, highlighting.Extensions, definition);
        }
    }

    private static SolidColorBrush Brush(string hex)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }
}