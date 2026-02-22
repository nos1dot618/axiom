using Axiom.UI.Themes;

namespace Axiom.Editor.Themes;

public interface IThemeService
{
    public IReadOnlyList<EditorTheme> Themes { get; }
    public EditorTheme CurrentTheme { get; }
    public void SetTheme(EditorTheme theme);
    public void SetTheme(string name);
}