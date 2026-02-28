using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Axiom.Common;
using Axiom.Editor;
using Axiom.Editor.Settings;
using Axiom.UI.Themes;
using ICSharpCode.AvalonEdit;

namespace Axiom.UI.Editor;

public static class EditorUiController
{
    public static ICommand ExitCommand { get; } = new RelayCommand(_ => Exit());
    public static ICommand ReloadCommand { get; } = new RelayCommand(_ => Reload());
    public static ICommand IncreaseFontSizeCommand { get; } = new RelayCommand(_ => IncreaseFontSize());
    public static ICommand DecreaseFontSizeCommand { get; } = new RelayCommand(_ => DecreaseFontSize());

    public static void Configure()
    {
        var editor = EditorService.Editor;
        var settings = ServicesRegistry.SettingsService.CurrentSettings;

        editor.Options.ConvertTabsToSpaces = settings.Editor.ConvertTabsToSpaces;
        editor.Options.IndentationSize = settings.Editor.IndentationSize;
        editor.Options.EnableHyperlinks = settings.Editor.EnableHyperlinks;
        editor.Options.HighlightCurrentLine = settings.Editor.HighlightCurrentLine;
        editor.Options.AllowScrollBelowDocument = settings.Editor.AllowScrollBelowDocument;
        editor.Options.ShowSpaces = settings.Editor.ShowSpaces;

        // Font must be present inside Fonts.SystemFontFamilies
        editor.FontFamily = new FontFamily(settings.Editor.FontFamily);
        editor.FontSize = settings.Editor.FontSize;

        editor.VerticalScrollBarVisibility = ParseScrollBarVisibility(settings.Editor.VerticalScrollBarVisibility);
        editor.HorizontalScrollBarVisibility = ParseScrollBarVisibility(settings.Editor.HorizontalScrollBarVisibility);

        ServicesRegistry.ThemeService.SetTheme(settings.Editor.Theme);
        ThemeApplicator.Apply();

        return;

        ScrollBarVisibility ParseScrollBarVisibility(bool visible)
        {
            return visible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }
    }

    public static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control || sender is not TextEditor editor) return;

        var zoomStep = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.Step;
        var minFontSize = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.MinFontSize;
        var maxFontSize = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.MaxFontSize;

        var newSize = editor.FontSize + (e.Delta > 0 ? zoomStep : -zoomStep);
        editor.FontSize = Math.Clamp(newSize, minFontSize, maxFontSize);
        ServicesRegistry.SettingsService.Update(settings => { settings.Editor.FontSize = newSize; });

        e.Handled = true; // Prevent normal scroll.
    }

    private static void Exit()
    {
        Application.Current.Shutdown();
    }

    public static void Reload()
    {
        ServicesRegistry.SettingsService = new SettingsService();
        Configure();
    }

    private static void IncreaseFontSize()
    {
        var zoomStep = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.Step;
        var maxFontSize = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.MaxFontSize;

        var newSize = Math.Min(EditorService.Editor.FontSize + zoomStep, maxFontSize);
        EditorService.Editor.FontSize = newSize;
        ServicesRegistry.SettingsService.Update(settings => { settings.Editor.FontSize = newSize; });
    }

    private static void DecreaseFontSize()
    {
        var zoomStep = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.Step;
        var minFontSize = ServicesRegistry.SettingsService.CurrentSettings.Editor.Appearance.Zoom.MinFontSize;

        var newSize = Math.Max(EditorService.Editor.FontSize - zoomStep, minFontSize);
        EditorService.Editor.FontSize = newSize;
        ServicesRegistry.SettingsService.Update(settings => { settings.Editor.FontSize = newSize; });
    }
}