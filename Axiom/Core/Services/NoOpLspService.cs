using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Core.Services;

public sealed class NoOpLspService : ILspService
{
    public LspCapabilities Capabilities { get; } = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<DocumentMetadata> OpenDocumentAsync(string filepath, string text) =>
        Task.FromResult<DocumentMetadata>(null!);

    public Task ChangeDocumentAsync(DocumentMetadata metadata, DocumentChangeDto changeDto) => Task.CompletedTask;

    public Task SaveDocumentAsync(DocumentMetadata metadata) => Task.CompletedTask;

    public Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(DocumentMetadata metadata, DocumentPosition position,
        CompletionContextDto contextDto) => Task.FromResult<IReadOnlyList<CompletionItem>>([]);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}