using Axiom.Completion;
using Axiom.Documents;
using Axiom.Lsp.Models;
using Axiom.Lsp.Protocol;
using Axiom.Lsp.Transport;

namespace Axiom.Lsp.Language;

public sealed class LspLanguageService : IAsyncDisposable
{
    private readonly LspServerConfiguration _configuration;
    private readonly JsonRpcLspClient _transport;
    private readonly LspProtocolClient _client;
    private LspCapabilities? _capabilities;

    public string LanguageId => _configuration.LanguageId;

    public LspLanguageService(LspServerConfiguration configuration)
    {
        _configuration = configuration;
        _transport = new JsonRpcLspClient(configuration);
        _client = new LspProtocolClient(_transport);
    }

    public async Task InitializeAsync()
    {
        await _transport.StartAsync();
        _capabilities = await _client.InitializeAsync();
    }

    public async Task<DocumentMetadata> OpenDocumentAsync(string filePath, string languageId, string text)
    {
        var uri = new Uri(filePath).AbsoluteUri;
        var documentMetadata = new DocumentMetadata(uri, languageId);

        await _client.DidOpenAsync(documentMetadata, text);
        return documentMetadata;
    }

    public async Task ChangeDocumentAsync(DocumentMetadata documentMetadata, LspDocumentChangeDto changeDto)
    {
        documentMetadata.IncrementVersion();

        await _client.DidChangeAsync(documentMetadata, changeDto);
    }

    public async Task SaveDocumentAsync(DocumentMetadata documentMetadata)
    {
        await _client.DidSaveAsync(documentMetadata);
    }

    public Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(DocumentMetadata documentMetadata,
        LspDocumentPosition position)
    {
        return _client.RequestCompletionItems(documentMetadata, position);
    }

    public async ValueTask DisposeAsync()
    {
        await _transport.DisposeAsync();
    }
}