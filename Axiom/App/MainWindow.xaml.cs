using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Axiom.Editor;
using Axiom.Infrastructure.Logging;
using Axiom.UI.Commands;
using Axiom.UI.Editor;

namespace Axiom.App;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        EditorService.Editor = Editor;
        EditorConfigurator.Configure(Editor);

        Loaded += (_, _) => AsyncCommand.Execute(ServicesRegistry.EditorService.OnLoadCallback);
        Closed += (_, _) => AsyncCommand.Execute(ServicesRegistry.EditorService.OnCloseCallback);
        Editor.Document.Changed += async (_, e) => await ServicesRegistry.EditorService.OnDocumentChangeCallback(e);

        SetKeybindings();
        SetDefaultOptions();
    }

    private void SetKeybindings()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
            (_, _) => AsyncCommand.Execute(ServicesRegistry.FileService.OpenFileDialogAsync)));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save,
            (_, _) => AsyncCommand.Execute(ServicesRegistry.FileService.SaveAsync)));

        InputBindings.Add(new KeyBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
    }

    private void SetDefaultOptions()
    {
        var settings = ServicesRegistry.SettingsService.CurrentSettings;

        LanguageServerEnabledMenuItem.IsChecked = settings.Lsp.EnableLsp;
        CodeCompletionMenuItem.IsChecked = settings.Lsp.EnableCodeCompletion;
        DiagnosticsMenuItem.IsChecked = settings.Lsp.EnableDiagnostics;
    }

    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OpenFileMenuButtonHandler(object? sender, RoutedEventArgs e)
    {
        AsyncCommand.Execute(ServicesRegistry.FileService.OpenFileDialogAsync);
    }

    private async void LanguageServerEnabledMenuItemHandler(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem) return;

            ServicesRegistry.SettingsService.Update(settings =>
            {
                settings.Lsp.EnableLsp = LanguageServerEnabledMenuItem.IsChecked;
            });

            await ServicesRegistry.EditorService.ToggleLsp();
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }

    private void LanguageServerFeatureMenuItemHandler(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem) return;

        ServicesRegistry.SettingsService.Update(settings =>
        {
            settings.Lsp.EnableCodeCompletion = CodeCompletionMenuItem.IsChecked;
            settings.Lsp.EnableDiagnostics = DiagnosticsMenuItem.IsChecked;
        });

        ServicesRegistry.LspSession.ToggleFeatures();
    }

    private void ExitMenuButtonHandler(object? sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}