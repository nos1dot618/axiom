using System.Windows.Controls;
using Axiom.Editor.Documents;
using Axiom.Editor.Lsp;
using Axiom.Infrastructure.Lsp.Language;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor;

public class EditorService : IEditorService
{
    private static TextEditor? _editor;

    public static TextEditor Editor
    {
        get => _editor ?? throw new InvalidOperationException("Editor has not been initialized.");
        set => _editor = value;
    }

    public async Task OnLoadCallback()
    {
        // TODO: Support loading the previous session.
        await ServicesRegistry.FileService.NewDocumentAsync();
    }

    public async Task OnCloseCallback()
    {
        await ServicesRegistry.LspSession.DisposeAsync();
        DocumentManager.CloseFile();
    }

    public async Task OnDocumentChangeCallback(DocumentChangeEventArgs e)
    {
        var editor = Editor;

        // Close tooltip if exists.
        if (editor.ToolTip != null)
        {
            ((ToolTip)editor.ToolTip).IsOpen = false;
            editor.ToolTip = null;
        }

        if (ServicesRegistry.FileService.DocumentMetadata == null ||
            ServicesRegistry.DocumentManager.SuppressChanges) return;

        var changeDto = DocumentManager.CreateChange(e);

        var lspService = ServicesRegistry.LspSession.LspService;
        await lspService.ChangeDocumentAsync(ServicesRegistry.FileService.DocumentMetadata, changeDto);
    }

    public async Task ToggleLsp()
    {
        if (!ServicesRegistry.SettingsService.CurrentSettings.Lsp.EnableLsp)
        {
            await LspSession.Reload(new NoOpLspService());
            return;
        }

        var languageId = LspSession.EffectiveLanguageId;
        if (languageId == null) return;
        var lspConfiguration = LspRegistry.Get(languageId);
        if (lspConfiguration == null) return;

        await LspSession.Reload(new LspService(lspConfiguration));
    }
}