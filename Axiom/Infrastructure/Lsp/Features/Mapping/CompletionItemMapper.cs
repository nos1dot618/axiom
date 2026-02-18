using System.Text.Json;
using Axiom.Core.Completion;

namespace Axiom.Infrastructure.Lsp.Features.Mapping;

public sealed class CompletionItemMapper : IFeatureMapper<CompletionItem>
{
    public string ResultSetName => "items";

    public CompletionItem MapSingle(JsonElement item)
    {
        var text = item.GetProperty("label").GetString() ?? "";
        var insertText = item.TryGetProperty("insertText", out var insertTextEx) ? insertTextEx.GetString() : text;
        return new CompletionItem(text, insertText ?? text);
    }
}