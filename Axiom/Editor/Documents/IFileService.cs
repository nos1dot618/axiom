using Axiom.Core.Documents;

namespace Axiom.Editor.Documents;

public interface IFileService
{
    /// <summary>
    ///     This is merely to be used for the LSP Session. This may not reflect the currently opened document.
    /// </summary>
    DocumentMetadata? DocumentMetadata { get; }

    Task OpenDocumentAsync(string filepath, string text);
    Task SaveAsync();
    Task OpenFileDialogAsync();
    Task NewDocumentAsync();
}