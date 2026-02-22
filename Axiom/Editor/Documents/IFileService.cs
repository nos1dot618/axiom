using Axiom.Core.Documents;

namespace Axiom.Editor.Documents;

public interface IFileService
{
    DocumentMetadata? DocumentMetadata { get; }

    Task OpenFileAsync(string filepath);
    Task OpenDocumentAsync(string filepath, string text);
    Task SaveAsync();
    Task OpenFileDialogAsync();
}