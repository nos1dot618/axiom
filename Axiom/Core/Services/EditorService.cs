using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor;
using Axiom.Editor.Completion;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public class EditorService(DocumentManager documentManager, IFileService fileService) : IEditorService
{
    // TODO: Add setting for default value.
    public bool IsLspEnabled { get; private set; } = true;

    // TODO: Hard coded.
    private readonly LspServerConfiguration _lspConfiguration = new(
        languageId: "python",
        command: "../../../../node_modules/.bin/pyright-langserver.cmd",
        arguments: "--stdio",
        rootPath: @"C:\Users\nosferatu\Downloads"
    );

    private CompletionEngine? _completionEngine;

    public async Task OnLoadCallback()
    {
        await RestartLspFeatures();

        // TODO: Replace with some temporary file, or load the previous session.
        await fileService.OpenFileAsync(@"C:\Users\nosferatu\Downloads\test.py");
    }

    public async Task OnCloseCallback()
    {
        _completionEngine?.Dispose();
        var lspService = ServiceFactory.LspService;
        await lspService.DisposeAsync();
        DocumentManager.CloseFile();
    }

    public async Task OnDocumentChangeCallback(DocumentChangeEventArgs e)
    {
        var editor = EditorContext.GetEditor();

        // Close tooltip if exists.
        if (editor.ToolTip != null)
        {
            ((System.Windows.Controls.ToolTip)editor.ToolTip).IsOpen = false;
            editor.ToolTip = null;
        }

        if (fileService.DocumentMetadata == null || documentManager.SuppressChanges) return;

        var changeDto = documentManager.CreateChange(e);

        var lspService = ServiceFactory.LspService;
        await lspService.ChangeDocumentAsync(fileService.DocumentMetadata, changeDto);
    }

    public async Task ToggleLsp()
    {
        await ServiceFactory.LspService.DisposeAsync();
        ServiceFactory.LspService = IsLspEnabled ? new NoOpLspService() : new LspService(_lspConfiguration);
        await RestartLspFeatures();
        IsLspEnabled = !IsLspEnabled;
    }

    private async Task RestartLspFeatures()
    {
        var lspService = ServiceFactory.LspService;
        await lspService.InitializeAsync();

        // TODO: Add feature to register CompletionEngine inside LspService,
        //       to sync the lifecycle of CompletionWindow with LspService.
        _completionEngine?.Dispose();
        _completionEngine = new CompletionEngine(
            EditorContext.GetEditor().TextArea,
            lspService.Capabilities.CompletionTriggerCharacters,
            CompletionProvider
        );
    }

    private async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        var lspService = ServiceFactory.LspService;
        if (fileService.DocumentMetadata == null) return [];

        DocumentPosition position = new(EditorContext.GetEditor().TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await lspService.GetCompletionsAsync(fileService.DocumentMetadata!, position, contextDto);
    }
}