using System.Windows;
using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;
using ICSharpCode.AvalonEdit;

namespace Axiom.UI.Editor;

public static class EditorUiController
{
    public static ICommand ExitCommand { get; } = new RelayCommand(_ => Exit());
    public static ICommand IncreaseFontSizeCommand { get; } = new RelayCommand(_ => IncreaseFontSize());
    public static ICommand DecreaseFontSizeCommand { get; } = new RelayCommand(_ => DecreaseFontSize());

    private static void Exit()
    {
        Application.Current.Shutdown();
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
}