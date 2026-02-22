using Axiom.UI.Themes;

namespace Axiom.Core.Services;

public interface IThemeService
{
    public IReadOnlyList<EditorTheme> Themes { get; }
    public EditorTheme CurrentTheme { get; }
    public void SetTheme(EditorTheme theme);
    public void SetTheme(string name);
}