using System.Windows;
using System.Windows.Input;
using Accessibility;
using Axiom.Core.Services;
using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor;
using Axiom.Editor.Completion;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Logging;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Commands;
using Axiom.UI.Editor;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.App;

public partial class MainWindow
{
    private readonly LspLanguageService? _lspService;
    private readonly DocumentManager _documentManager;
    private readonly IFileService _fileService;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IEditorService _editorService;

    public MainWindow()
    {
        InitializeComponent();

        EditorContext.SetEditor(Editor);
        EditorConfigurator.Configure(Editor);
        SetKeybindings();

        LspServerConfiguration lspConfiguration = new(
            languageId: "python",
            command: "../../../../node_modules/.bin/pyright-langserver.cmd",
            arguments: "--stdio",
            rootPath: @"C:\Users\nosferatu\Downloads"
        );

        _lspService = new LspLanguageService(lspConfiguration);
        _documentManager = new DocumentManager(Editor);
        ServiceFactory.Configure(_documentManager, _lspService);
        _fileService = ServiceFactory.FileService;
        _editorService = ServiceFactory.EditorService;

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

    private void ExitMenuButtonHandler(object? sender, RoutedEventArgs e)
    {
    }
}