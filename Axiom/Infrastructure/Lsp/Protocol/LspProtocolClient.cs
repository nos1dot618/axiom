using System.Diagnostics.CodeAnalysis;
using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor;
using Axiom.Infrastructure.Lsp.Features.Mapping;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.Infrastructure.Lsp.Transport;

namespace Axiom.Infrastructure.Lsp.Protocol;

public sealed class LspProtocolClient(JsonRpcLspClient transport)
{
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private readonly IFeatureMapper<CompletionItem> _mapper = new CompletionItemMapper();

    public async Task<LspCapabilities> InitializeAsync()
    {
        var rootUri = new Uri(ServicesRegistry.SettingsService.CurrentSettings.Lsp.DefaultRootMode.GetRootPath())
            .AbsoluteUri;

        var result = await transport.SendRequestAsync(LspMethod.Request.Initialize, new
        {
            processId = Environment.ProcessId,
            rootUri,
            workspaceFolders = new[]
            {
                new { uri = rootUri, name = "workspace" }
            },
            capabilities = new
            {
                textDocument = new
                {
                    synchronization = new
                    {
                        didSave = true,
                        willSave = false,
                        willSaveWaitUntil = false,
                        dynamicRegistration = false
                    },
                    completion = new
                    {
                        dynamicRegistration = false,
                        completionItem = new { snippetSupport = true }
                    }
                }
            }
        });

        await transport.SendNotificationAsync(LspMethod.Notification.Initialized);

        return new LspCapabilities(result);
    }

    public Task DidOpenAsync(DocumentMetadata documentMetadata, string text)
    {
        return transport.SendNotificationAsync(LspMethod.Notification.DidOpen, new
        {
            textDocument = new
            {
                uri = documentMetadata.Uri,
                languageId = documentMetadata.LanguageId,
                version = documentMetadata.Version,
                text
            }
        });
    }

    public Task DidChangeAsync(DocumentMetadata documentMetadata, DocumentChangeDto changeDto)
    {
        return transport.SendNotificationAsync(LspMethod.Notification.DidChange, new
        {
            textDocument = new
            {
                uri = documentMetadata.Uri,
                version = documentMetadata.Version
            },
            contentChanges = new[]
            {
                new
                {
                    range = new
                    {
                        start = new { line = changeDto.Start.Row, character = changeDto.Start.Column },
                        end = new { line = changeDto.End.Row, character = changeDto.End.Column }
                    },
                    text = changeDto.Text
                }
            }
        });
    }

    public Task DidSaveAsync(DocumentMetadata documentMetadata)
    {
        return transport.SendNotificationAsync(LspMethod.Notification.DidSave, new
        {
            textDocument = new { uri = documentMetadata.Uri }
        });
    }

    // TODO: Create LspRequestMethodHandler similar to LspNotificationHandler.
    public async Task<IReadOnlyList<CompletionItem>> RequestCompletionItems(DocumentMetadata documentMetadata,
        DocumentPosition position, CompletionContextDto contextDto)
    {
        var result = await transport.SendRequestAsync(LspMethod.Request.TextCompletion, new
        {
            textDocument = new { uri = documentMetadata.Uri },
            position = position.ToDto(),
            context = contextDto.ToDto()
        });

        return _mapper.Map(result);
    }
}