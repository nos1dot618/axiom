using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Infrastructure.Lsp.Features.Dispatching;
using Axiom.Infrastructure.Lsp.Protocol;
using Axiom.Infrastructure.Lsp.Transport;

namespace Axiom.Infrastructure.Lsp.Language;

public sealed class LspLanguageService : IAsyncDisposable
{
    private readonly LspServerConfiguration _configuration;
    private readonly JsonRpcLspClient _transport;
    private readonly LspProtocolClient _client;

    public LspCapabilities Capabilities { get; private set; } = new();
    public string LanguageId => _configuration.LanguageId;

    public LspLanguageService(LspServerConfiguration configuration)
    {
        _configuration = configuration;
        _transport = new JsonRpcLspClient(configuration);
        _client = new LspProtocolClient(_transport);

        RegisterHandlers();
    }

    public async Task InitializeAsync()
    {
        await _transport.StartAsync();
        Capabilities = await _client.InitializeAsync();
    }

    public async Task<DocumentMetadata> OpenDocumentAsync(string filePath, string languageId, string text)
    {
        var uri = new Uri(filePath).AbsoluteUri;
        var documentMetadata = new DocumentMetadata(uri, languageId);

        await _client.DidOpenAsync(documentMetadata, text);
        return documentMetadata;
    }

    public async Task ChangeDocumentAsync(DocumentMetadata documentMetadata, DocumentChangeDto changeDto)
    {
        documentMetadata.IncrementVersion();

        await _client.DidChangeAsync(documentMetadata, changeDto);
    }

    public async Task SaveDocumentAsync(DocumentMetadata documentMetadata)
    {
        await _client.DidSaveAsync(documentMetadata);
    }

    public Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(DocumentMetadata documentMetadata,
        DocumentPosition position, CompletionContextDto contextDto)
    {
        return _client.RequestCompletionItems(documentMetadata, position, contextDto);
    }

    public async ValueTask DisposeAsync()
    {
        await _transport.DisposeAsync();
    }

    private void RegisterHandlers()
    {
        RegisterNotificationHandler(new DiagnosticsNotificationHandler());
    }

    private void RegisterNotificationHandler(ILspNotificationHandler handler) =>
        _transport.RegisterNotificationHandler(handler.Method, payload => handler.HandleAsync(payload));
}