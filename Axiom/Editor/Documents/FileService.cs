using System.IO;
using Axiom.Core.Documents;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Themes;
using Microsoft.Win32;
using Buffer = Axiom.Core.Documents.Buffer;

namespace Axiom.Editor.Documents;

public class FileService : IFileService
{
    public static Buffer CurrentBuffer { get; private set; } = new();

    public string ProjectRoot =>
        ServicesRegistry.SettingsService.CurrentSettings.Lsp.DefaultRootMode.GetRootPath();

    public DocumentMetadata? DocumentMetadata { get; set; }

    public async Task OpenDocumentAsync(string filepath, string text)
    {
        DocumentMetadata = await ServicesRegistry.LspSession.LspService.OpenDocumentAsync(filepath, text);
        CurrentBuffer.Change(filepath);
    }

    public async Task SaveAsync()
    {
        // Save document when the Editor buffer is non-empty.
        // TODO: Check whether the document needs saving or not. If no new changes then saving can be skipped.
        //       We can compute digest for determining.
        if (string.IsNullOrEmpty(EditorService.Editor.Text)) return;

        if (CurrentBuffer.IsVirtual)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true) CurrentBuffer.Change(dialog.FileName);
            else return;
            await SetupCurrentDocument();
        }

        if (CurrentBuffer.IsVirtual) return;

        await File.WriteAllTextAsync(CurrentBuffer.Path, EditorService.Editor.Text);

        if (DocumentMetadata == null || string.IsNullOrEmpty(DocumentMetadata.Uri)) return;
        await ServicesRegistry.LspSession.LspService.SaveDocumentAsync(DocumentMetadata);
    }

    public async Task OpenFileDialogAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*"
        };

        // TODO: Save file if any changes before switching to a different file.
        if (dialog.ShowDialog() == true) await OpenFileAsync(dialog.FileName);
    }

    public async Task NewDocumentAsync()
    {
        await SaveAsync();
        await ServicesRegistry.LspSession.DisposeAsync();
        ThemeApplicator.RemoveSyntaxHighlighting();

        CurrentBuffer = new Buffer();
        EditorService.Editor.Text = string.Empty;
    }

    private static async Task OpenFileAsync(string filepath)
    {
        await ServicesRegistry.DocumentManager.LoadFileAsync(filepath);
        CurrentBuffer.Change(filepath);
        await SetupCurrentDocument();
    }

    private static async Task SetupCurrentDocument()
    {
        if (CurrentBuffer.LanguageId != null)
            await ServicesRegistry.EditorService.SetLanguage(CurrentBuffer.LanguageId);
    }
}