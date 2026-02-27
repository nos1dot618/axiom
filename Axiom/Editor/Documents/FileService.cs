using System.IO;
using Axiom.Core.Documents;
using Axiom.Editor.Lsp;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Themes;
using Microsoft.Win32;

namespace Axiom.Editor.Documents;

public class FileService : IFileService
{
    public static DocumentAddress CurrentDocumentAddress { get; private set; } = new();
    public static string? WorkingDirectory { get; private set; }

    public DocumentMetadata? DocumentMetadata { get; private set; }

    public async Task OpenDocumentAsync(string filepath, string text)
    {
        DocumentMetadata = await ServicesRegistry.LspSession.LspService.OpenDocumentAsync(filepath, text);
        CurrentDocumentAddress = new DocumentAddress(filepath);
        WorkingDirectory = Path.GetDirectoryName(CurrentDocumentAddress.Path);
    }

    public async Task SaveAsync()
    {
        // Save document when the Editor buffer is non-empty.
        // TODO: Check whether the document needs saving or not. If no new changes then saving can be skipped.
        //       We can compute digest for determining.
        if (string.IsNullOrEmpty(EditorService.Editor.Text)) return;

        if (CurrentDocumentAddress.IsVirtual)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true) CurrentDocumentAddress = new DocumentAddress(dialog.FileName);
            else return;
            await SetupCurrentDocument(EditorService.Editor.Text);
        }

        if (CurrentDocumentAddress.IsVirtual) return;

        await File.WriteAllTextAsync(CurrentDocumentAddress.Path, EditorService.Editor.Text);

        if (DocumentMetadata == null || string.IsNullOrEmpty(DocumentMetadata.Uri)) return;
        await ServicesRegistry.LspSession.LspService.SaveDocumentAsync(DocumentMetadata);
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

    public async Task NewDocumentAsync()
    {
        await SaveAsync();
        await ServicesRegistry.LspSession.DisposeAsync();
        ThemeApplicator.RemoveSyntaxHighlighting();

        LspSession.LanguageId = null;
        CurrentDocumentAddress = new DocumentAddress();
        EditorService.Editor.Text = string.Empty;
    }

    private async Task OpenFileAsync(string filepath)
    {
        var contents = await ServicesRegistry.DocumentManager.LoadFileAsync(filepath);
        CurrentDocumentAddress = new DocumentAddress(filepath);
        await SetupCurrentDocument(contents);
    }

    /// <summary>
    ///     Sets up Working Directory, LSP and Syntax Highlighting.
    /// </summary>
    /// <param name="contents">Contents of the Current Document.</param>
    private async Task SetupCurrentDocument(string contents)
    {
        WorkingDirectory = ServicesRegistry.SettingsService.CurrentSettings.Lsp.DefaultRootMode.GetRootPath();

        var languageId = LanguageIdResolver.GetLanguageId(CurrentDocumentAddress.Path);
        // If the language ID of the newly opened document is same as the current running LSP server's language ID:
        // them no need to change the LSP session.
        if (languageId == LspSession.LanguageId)
        {
            DocumentMetadata =
                await ServicesRegistry.LspSession.LspService.OpenDocumentAsync(CurrentDocumentAddress.Path, contents);
            return;
        }

        DocumentMetadata = null;
        if (languageId == null) return;
        var lspConfiguration = LspRegistry.Get(languageId);
        if (lspConfiguration == null) return;

        await LspSession.Reload(new LspService(lspConfiguration));
        ThemeApplicator.ApplySyntaxHighlighting();
    }
}