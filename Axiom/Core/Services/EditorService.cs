using System.Windows.Controls;
using Axiom.Editor;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public class EditorService : IEditorService
{
    // TODO: Hard coded.
    private readonly LspServerConfiguration _lspConfiguration = new(
        "python",
        "../../../../node_modules/.bin/pyright-langserver.cmd",
        "--stdio",
        @"C:\Users\nosferatu\Downloads"
    );

    // TODO: Add setting for default value.
    public bool IsLspEnabled { get; private set; } = true;

    public async Task OnLoadCallback()
    {
        await ServiceFactory.Configure(new LspService(_lspConfiguration));

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

        var changeDto = ServiceFactory.DocumentManager.CreateChange(e);

        var lspService = ServiceFactory.LspSession.LspService;
        await lspService.ChangeDocumentAsync(ServiceFactory.FileService.DocumentMetadata, changeDto);
    }

    public async Task ToggleLsp()
    {
        await ServiceFactory.LspSession.DisposeAsync();

        ILspService lspService = IsLspEnabled ? new NoOpLspService() : new LspService(_lspConfiguration);
        ServiceFactory.LspSession = new LspSession(lspService);
        await ServiceFactory.LspSession.InitializeAsync();
        if (DocumentManager.CurrentDocumentUri is not null)
            await lspService.OpenDocumentAsync(new Uri(DocumentManager.CurrentDocumentUri).LocalPath,
                EditorContext.GetEditor().Text);
        IsLspEnabled = !IsLspEnabled;
    }
}