using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;
using Axiom.UI;
using Axiom.UI.Editor;

namespace Axiom.App;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        EditorService.Editor = Editor;

        Loaded += (_, _) => AsyncCommand.Execute(ServicesRegistry.EditorService.OnLoadCallback);
        Closed += (_, _) => AsyncCommand.Execute(ServicesRegistry.EditorService.OnCloseCallback);
        Editor.Document.Changed += async (_, e) => await ServicesRegistry.EditorService.OnDocumentChangeCallback(e);

        SetKeybindings();
        SetDefaultOptions();
        UiControllersRegistry.Configure(ThemesMenuItem, LanguageMenuItem, TimerText, StatusText);
    }

    private void SetKeybindings()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
            (_, _) => AsyncCommand.Execute(ServicesRegistry.FileService.OpenFileDialogAsync)));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save,
            (_, _) => AsyncCommand.Execute(ServicesRegistry.FileService.SaveAsync)));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.New,
            (_, _) => AsyncCommand.Execute(ServicesRegistry.FileService.NewDocumentAsync)));

        InputBindings.Add(new KeyBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.New, new KeyGesture(Key.N, ModifierKeys.Control)));
    }

    private void SetDefaultOptions()
    {
        var settings = ServicesRegistry.SettingsService.CurrentSettings;

        LanguageServerEnabledMenuItem.IsChecked = settings.Lsp.EnableLsp;
        CodeCompletionMenuItem.IsChecked = settings.Lsp.EnableCodeCompletion;
        DiagnosticsMenuItem.IsChecked = settings.Lsp.EnableDiagnostics;
    }

    private void EditorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        EditorUiController.HandlePreviewMouseWheel(sender, e);
    }
}