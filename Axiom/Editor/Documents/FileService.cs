using System.IO;
using Axiom.Core.Documents;
using Axiom.Editor.Lsp;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Themes;
using Microsoft.Win32;

namespace Axiom.Editor.Documents;

public class FileService : IFileService
{
    public DocumentMetadata? DocumentMetadata { get; private set; }

    public async Task OpenFileAsync(string filepath)
    {
        var text = await ServiceFactory.DocumentManager.LoadFileAsync(filepath);

        var languageId = LanguageIdResolver.GetLanguageId(filepath);
        // If the language ID of the newly opened document is same as the current running LSP server's language ID:
        // them no need to change the LSP session.
        if (languageId == LspSession.LanguageId)
        {
            DocumentMetadata = await ServiceFactory.LspSession.LspService.OpenDocumentAsync(filepath, text);
            return;
        }

        if (languageId == null) return;
        var lspConfiguration = LspRegistry.Get(languageId);
        if (lspConfiguration == null) return;

        await LspSession.Reload(new LspService(lspConfiguration));

        ThemeApplicator.ApplySyntaxHighlighting();
    }

    public async Task OpenDocumentAsync(string filepath, string text)
    {
        DocumentMetadata = await ServiceFactory.LspSession.LspService.OpenDocumentAsync(filepath, text);
    }

    public async Task SaveAsync()
    {
        if (DocumentMetadata == null || string.IsNullOrEmpty(DocumentMetadata.Uri)) return;

        var filePath = new Uri(DocumentMetadata.Uri).LocalPath;
        await File.WriteAllTextAsync(filePath, EditorContext.GetEditor().Text);

        await ServiceFactory.LspSession.LspService.SaveDocumentAsync(DocumentMetadata);
    }

    public async Task OpenFileDialogAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*"
        };

        // TODO: Save file if any changes before switching to a different file.
        if (dialog.ShowDialog() == true) await OpenFileAsync(dialog.FileName);
    }
}