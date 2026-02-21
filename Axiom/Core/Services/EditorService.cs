using System.Windows.Controls;
using Axiom.Editor;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public class EditorService : IEditorService
{
    public async Task OnLoadCallback()
    {
        // TODO: Replace with some temporary file, or load the previous session.
        await ServiceFactory.FileService.OpenFileAsync(@"C:\Users\nosferatu\Downloads\test.py");
    }

    public async Task OnCloseCallback()
    {
        await ServiceFactory.LspSession.DisposeAsync();
        DocumentManager.CloseFile();
    }

    public async Task OnDocumentChangeCallback(DocumentChangeEventArgs e)
    {
        var editor = EditorContext.GetEditor();

        // Close tooltip if exists.
        if (editor.ToolTip != null)
        {
            ((ToolTip)editor.ToolTip).IsOpen = false;
            editor.ToolTip = null;
        }

        if (ServiceFactory.FileService.DocumentMetadata == null ||
            ServiceFactory.DocumentManager.SuppressChanges) return;

        var changeDto = DocumentManager.CreateChange(e);

        var lspService = ServiceFactory.LspSession.LspService;
        await lspService.ChangeDocumentAsync(ServiceFactory.FileService.DocumentMetadata, changeDto);
    }

    public async Task ToggleLsp()
    {
        ILspService lspService = ServiceFactory.SettingsService.CurrentSettings.Lsp.EnableLsp
            ? new LspService(LspRegistry.Get(LspSession.EffectiveLanguageId!)!)
            : new NoOpLspService();

        await LspSession.Reload(lspService);
    }
}