using System.Windows.Controls;
using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;

namespace Axiom.UI.Lsp;

public static class LspUiController
{
    public static ICommand ToggleLspCommand { get; } = new AsyncRelayCommand(ToggleLspAsync);
    public static ICommand ToggleFeatureCommand { get; } = new RelayCommand(ToggleFeature);

    private static async Task ToggleLspAsync(object? param)
    {
        if (param is not MenuItem menuItem) return;

        ServicesRegistry.SettingsService.Update(settings =>
        {
            settings.Lsp.EnableLsp = menuItem is { Name: "LanguageServerEnabledMenuItem", IsChecked: true };
        });

        await ServicesRegistry.EditorService.ToggleLsp();
    }

    private static void ToggleFeature(object? param)
    {
        if (param is not MenuItem menuItem) return;

        ServicesRegistry.SettingsService.Update(settings =>
        {
            settings.Lsp.EnableCodeCompletion = menuItem is { Name: "CodeCompletionMenuItem", IsChecked: true };
            settings.Lsp.EnableDiagnostics = menuItem is { Name: "DiagnosticsMenuItem", IsChecked: true };
        });

        ServicesRegistry.LspSession.ToggleFeatures();
    }
}