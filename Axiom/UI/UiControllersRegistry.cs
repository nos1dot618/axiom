using System.Windows.Controls;
using Axiom.UI.Editor;
using Axiom.UI.Project;
using Axiom.UI.Themes;

namespace Axiom.UI;

public static class UiControllersRegistry
{
    private static ThemeUiController? _themeUiController;
    private static LanguageUiController? _languageUiController;

    public static ThemeUiController Theme => _themeUiController ?? throw new NullReferenceException();
    public static LanguageUiController Language => _languageUiController ?? throw new NullReferenceException();

    public static void Configure(MenuItem themesMenuItem, MenuItem languagesMenuItem, TextBlock timerText,
        TextBlock statusText)
    {
        _themeUiController = new ThemeUiController(themesMenuItem);
        _languageUiController = new LanguageUiController(languagesMenuItem);
        _ = new ProjectUiController(timerText, statusText);
        EditorUiController.Configure();

        _themeUiController.Populate();
        _languageUiController.Populate();
    }
}