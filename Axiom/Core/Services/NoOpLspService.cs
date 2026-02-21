using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Core.Services;

public sealed class NoOpLspService : ILspService
{
    public LspCapabilities Capabilities { get; } = new();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task<DocumentMetadata> OpenDocumentAsync(string filepath, string text)
    {
        return Task.FromResult<DocumentMetadata>(null!);
    }

    public Task ChangeDocumentAsync(DocumentMetadata metadata, DocumentChangeDto changeDto)
    {
        return Task.CompletedTask;
    }

    public Task SaveDocumentAsync(DocumentMetadata metadata)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(DocumentMetadata metadata, DocumentPosition position,
        CompletionContextDto contextDto)
    {
        return Task.FromResult<IReadOnlyList<CompletionItem>>([]);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}