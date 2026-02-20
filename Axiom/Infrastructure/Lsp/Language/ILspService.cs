using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Infrastructure.Lsp.Language;

public interface ILspService : IAsyncDisposable
{
    LspCapabilities Capabilities { get; }

    Task InitializeAsync();
    Task<DocumentMetadata> OpenDocumentAsync(string filepath, string text);
    Task ChangeDocumentAsync(DocumentMetadata metadata, DocumentChangeDto changeDto);
    Task SaveDocumentAsync(DocumentMetadata metadata);

    Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(DocumentMetadata metadata, DocumentPosition position,
        CompletionContextDto contextDto);
}