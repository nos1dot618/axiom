using System.Text.Json;

namespace Axiom.Infrastructure.Lsp.Protocol;

public sealed class LspCapabilities
{
    private bool SupportsCompletion { get; set; }
    public bool SupportsCompletionResolve { get; private set; }
    public IReadOnlyList<string> CompletionTriggerCharacters { get; private set; } = [];

    public bool SupportsHover { get; private set; }
    public bool SupportsFormatting { get; private set; }
    public bool SupportsIncrementalSync { get; private set; }

    public LspCapabilities()
    {
    }

    public LspCapabilities(JsonElement result)
    {
        if (!result.TryGetProperty("capabilities", out var capabilitiesObject)) return;

        ParseCompletionElement(capabilitiesObject);
        SupportsHover = capabilitiesObject.TryGetProperty("hoverProvider", out _);
        SupportsFormatting = capabilitiesObject.TryGetProperty("documentFormattingProvider", out _);
        SupportsIncrementalSync =
            capabilitiesObject.TryGetProperty("textDocumentSync", out var sync) &&
            sync.ValueKind == JsonValueKind.Number &&
            sync.GetInt32() == 2;
    }

    private void ParseCompletionElement(JsonElement result)
    {
        SupportsCompletion = result.TryGetProperty("completionProvider", out var completionProvider);
        var completionTriggerCharacters = new List<string>();

        if (!SupportsCompletion) return;

        if (completionProvider.TryGetProperty("triggerCharacters", out var triggers) &&
            triggers.ValueKind == JsonValueKind.Array)
        {
            completionTriggerCharacters.AddRange(from item in triggers.EnumerateArray()
                where item.ValueKind == JsonValueKind.String
                select item.GetString()!);
        }

        CompletionTriggerCharacters = completionTriggerCharacters;
        if (completionProvider.TryGetProperty("resolveProvider", out var resolve))
        {
            SupportsCompletionResolve = resolve.GetBoolean();
        }
    }
}