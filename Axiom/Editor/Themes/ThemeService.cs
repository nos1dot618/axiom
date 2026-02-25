using System.IO;
using Axiom.UI.Themes;
using Tomlyn;

namespace Axiom.Editor.Themes;

public class ThemeService : IThemeService
{
    private static readonly string ThemesDirectoryPath = InitializeDirectoryPath();
    private readonly List<EditorTheme> _themes = [];

    public ThemeService()
    {
        foreach (var file in Directory.GetFiles(ThemesDirectoryPath, "*.toml"))
        {
            var text = File.ReadAllText(file);
            var theme = Toml.ToModel<EditorTheme>(text);
            _themes.Add(theme);
        }
    }

    public IReadOnlyList<EditorTheme> Themes => _themes;

    public EditorTheme CurrentTheme { get; private set; } = new();

    public void SetTheme(EditorTheme theme)
    {
        CurrentTheme = theme;
        ServicesRegistry.SettingsService.Update(settings => { settings.Editor.Theme = theme.Name; });
    }

    public void SetTheme(string name)
    {
        var theme = _themes.FirstOrDefault(theme =>
            string.Equals(theme.Name, name, StringComparison.OrdinalIgnoreCase));
        SetTheme(theme ?? new EditorTheme());
    }

    private static string InitializeDirectoryPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var themesDirectoryPath = Path.Combine(appDataPath, "Axiom", "Themes");
        // TODO: Maybe populate the themes directory with some default themes.
        Directory.CreateDirectory(themesDirectoryPath);

        return themesDirectoryPath;
    }
}