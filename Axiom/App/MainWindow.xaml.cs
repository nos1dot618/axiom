using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Axiom.Core.Services;
using Axiom.Editor;
using Axiom.Infrastructure.Logging;
using Axiom.UI.Commands;
using Axiom.UI.Editor;

namespace Axiom.App;

public partial class MainWindow
{
    private readonly IFileService _fileService;

    public MainWindow()
    {
        InitializeComponent();

        EditorContext.SetEditor(Editor);
        EditorConfigurator.Configure(Editor);
        SetKeybindings();

        _fileService = ServiceFactory.FileService;
        var editorService = ServiceFactory.EditorService;

        Loaded += (_, _) => AsyncCommand.Execute(editorService.OnLoadCallback);
        Closed += (_, _) => AsyncCommand.Execute(editorService.OnCloseCallback);
        Editor.Document.Changed += async (_, e) => await editorService.OnDocumentChangeCallback(e);
    }

    private void SetKeybindings()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
            (_, _) => AsyncCommand.Execute(_fileService.OpenFileDialogAsync)));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save,
            (_, _) => AsyncCommand.Execute(_fileService.SaveAsync)));

        InputBindings.Add(new KeyBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
    }

    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OpenFileMenuButtonHandler(object? sender, RoutedEventArgs e)
    {
        AsyncCommand.Execute(_fileService.OpenFileDialogAsync);
    }

    private async void ToggleLanguageServerButtonHandler(object? sender, RoutedEventArgs e)
    {
        try
        {
            // TODO: menuItem state is not used.
            if (sender is not MenuItem) return;

            await ServiceFactory.EditorService.ToggleLsp();
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }

    private void ExitMenuButtonHandler(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}