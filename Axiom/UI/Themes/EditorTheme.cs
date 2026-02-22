namespace Axiom.UI.Themes;

public sealed class EditorTheme
{
    public string Name { get; set; } = "Unnamed";
    public EditorColors Editor { get; set; } = new();
    public SyntaxColors Syntax { get; set; } = new();
}