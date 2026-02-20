using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Axiom.Core.Services;
using Axiom.Editor;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Logging;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Commands;
using Axiom.UI.Editor;

namespace Axiom.App;

public partial class MainWindow
{
    private readonly IFileService _fileService;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IEditorService _editorService;

    public MainWindow()
    {
        InitializeComponent();

        var lspConfiguration = new LspServerConfiguration(
            languageId: "python",
            command: "../../../../node_modules/.bin/pyright-langserver.cmd",
            arguments: "--stdio",
            rootPath: @"C:\Users\nosferatu\Downloads"
        );

        var lspService = new LspService(lspConfiguration);
        var documentManager = new DocumentManager(Editor);
        ServiceFactory.Configure(documentManager, lspService);
        _fileService = ServiceFactory.FileService;
        _editorService = ServiceFactory.EditorService;

        EditorContext.SetEditor(Editor);
        EditorConfigurator.Configure(Editor);
        SetKeybindings();

        Loaded += (_, _) => AsyncCommand.Execute(_editorService.OnLoadCallback);
        Closed += (_, _) => AsyncCommand.Execute(_editorService.OnCloseCallback);
        Editor.Document.Changed += async (_, e) => await _editorService.OnDocumentChangeCallback(e);
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

    private void OpenFileMenuButtonHandler(object? sender, RoutedEventArgs e) =>
        AsyncCommand.Execute(_fileService.OpenFileDialogAsync);

    private async void ToggleLanguageServerButtonHandler(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            Console.WriteLine(menuItem.IsChecked);

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