using Axiom.Core.Documents;

namespace Axiom.Core.Services;

public interface IFileService
{
    DocumentMetadata? DocumentMetadata { get; }

    Task OpenFileAsync(string filepath);
    Task OpenDocumentAsync(string filepath, string text);
    Task SaveAsync();
    Task OpenFileDialogAsync();
}