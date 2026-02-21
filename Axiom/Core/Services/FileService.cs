using System.IO;
using Axiom.Core.Documents;
using Axiom.Editor;
using Microsoft.Win32;

namespace Axiom.Core.Services;

public class FileService : IFileService
{
    public DocumentMetadata? DocumentMetadata { get; private set; }

    public async Task OpenFileAsync(string filepath)
    {
        var text = await ServiceFactory.DocumentManager.LoadFileAsync(filepath);
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