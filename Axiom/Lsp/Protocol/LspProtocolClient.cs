using System.Text.Json;
using Axiom.Completion;
using Axiom.Documents;
using Axiom.Lsp.Models;
using Axiom.Lsp.Transport;

namespace Axiom.Lsp.Protocol;

public sealed class LspProtocolClient(JsonRpcLspClient transport)
{
    public async Task<LspCapabilities> InitializeAsync()
    {
        var result = await transport.SendRequestAsync(LspMethod.Request.Initialize, new
        {
            processId = Environment.ProcessId,
            rootUri = transport.Configuration.RootPath,
            workspaceFolders = new[]
            {
                new { uri = transport.Configuration.RootPath, name = "workspace" }
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
                    }
                }
            }
        });

        await transport.SendNotificationAsync(LspMethod.Notification.Initialized);

        if (result.TryGetProperty("capabilities", out var capabilitiesObject))
        {
            return new LspCapabilities
            {
                SupportsCompletion = capabilitiesObject.TryGetProperty("completionProvider", out _),
                SupportsHover = capabilitiesObject.TryGetProperty("hoverProvider", out _),
                SupportsFormatting = capabilitiesObject.TryGetProperty("documentFormattingProvider", out _),
                SupportsIncrementalSync =
                    capabilitiesObject.TryGetProperty("textDocumentSync", out var sync) &&
                    sync.ValueKind == JsonValueKind.Number &&
                    sync.GetInt32() == 2
            };
        }

        return new LspCapabilities();
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

    public Task DidChangeAsync(DocumentMetadata documentMetadata, LspDocumentChangeDto changeDto)
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

    public async Task<IReadOnlyList<CompletionItem>> RequestCompletionItems(DocumentMetadata documentMetadata,
        LspDocumentPosition position)
    {
        var result = await transport.SendRequestAsync(LspMethod.Request.TextCompletion, new
        {
            textDocument = new { uri = documentMetadata.Uri },
            position = position.ToDto()
        });

        return CompletionItemMapper.Map(result);
    }
}