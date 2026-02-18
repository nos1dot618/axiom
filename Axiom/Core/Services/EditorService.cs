using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor;
using Axiom.Editor.Completion;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public class EditorService(DocumentManager documentManager, LspLanguageService? lspService, IFileService fileService)
    : IEditorService
{
    public async Task OnLoadCallback()
    {
        if (lspService != null)
        {
            await lspService.InitializeAsync();

            _ = new CompletionEngine(
                EditorContext.GetEditor().TextArea,
                lspService!.Capabilities.CompletionTriggerCharacters,
                CompletionProvider
            );
        }

        // TODO: Replace with some temporary file, or load the previous session.
        await fileService.OpenFileAsync(@"C:\Users\nosferatu\Downloads\test.py");
    }

    public async Task OnCloseCallback()
    {
        if (lspService != null) await lspService.DisposeAsync();
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

        if (lspService != null) await lspService.ChangeDocumentAsync(fileService.DocumentMetadata, changeDto);
    }

    private async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        if (fileService.DocumentMetadata == null && lspService == null) return [];

        DocumentPosition position = new(EditorContext.GetEditor().TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await lspService!.GetCompletionsAsync(fileService.DocumentMetadata!, position, contextDto);
    }
}