using System.Windows;
using System.Windows.Input;
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

    public MainWindow()
    {
        InitializeComponent();
        EditorContext.SetEditor(Editor);

        LspServerConfiguration lspConfiguration = new(
            languageId: "python",
            command: "../../../../node_modules/.bin/pyright-langserver.cmd",
            arguments: "--stdio",
            rootPath: @"C:\Users\nosferatu\Downloads"
        );

        _lspService = new LspLanguageService(lspConfiguration);
        _documentManager = new DocumentManager(Editor);
        _fileService = new FileService(_documentManager, _lspService);

        EditorConfigurator.Configure(Editor);
        SetKeybindings();
        EditorContext.SetEditor(Editor);

        Editor.Text = "";

        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
        Editor.Document.Changed += OnDocumentChanged;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_lspService != null)
            {
                await _lspService.InitializeAsync();

                _ = new CompletionEngine(
                    Editor.TextArea,
                    _lspService!.Capabilities.CompletionTriggerCharacters,
                    CompletionProvider
                );
            }

            await _fileService.OpenFileAsync(@"C:\Users\nosferatu\Downloads\test.py");
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }

    private async void OnClosedAsync(object? sender, EventArgs e)
    {
        try
        {
            if (_lspService != null) await _lspService.DisposeAsync();
            DocumentManager.CloseFile();
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }

    private async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        if (_fileService.DocumentMetadata == null && _lspService == null) return [];

        DocumentPosition position = new(Editor.TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await _lspService!.GetCompletionsAsync(_fileService.DocumentMetadata!, position, contextDto);
    }

    private async void OnDocumentChanged(object? sender, DocumentChangeEventArgs e)
    {
        try
        {
            // Close tooltip if exists.
            if (Editor.ToolTip != null)
            {
                ((System.Windows.Controls.ToolTip)Editor.ToolTip).IsOpen = false;
                Editor.ToolTip = null;
            }

            if (_fileService.DocumentMetadata == null || _documentManager.SuppressChanges) return;

            var changeDto = _documentManager.CreateChange(e);

            if (_lspService != null) await _lspService.ChangeDocumentAsync(_fileService.DocumentMetadata, changeDto);
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }

    private void SetKeybindings()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
            AsyncCommand.Create(_fileService.OpenFileDialogAsync)));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, AsyncCommand.Create(_fileService.SaveAsync)));

        InputBindings.Add(new KeyBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
    }

    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}