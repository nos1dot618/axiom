using System.Text.Json;

namespace Axiom.Core.Completion;

public sealed class CompletionItemMapper : IServiceMapper<CompletionItem>
{
    public string ResultSetName => "items";

    public CompletionItem MapSingle(JsonElement item)
    {
        var text = item.GetProperty("label").GetString() ?? "";
        var insertText = item.TryGetProperty("insertText", out var insertTextEx)
            ? insertTextEx.GetString()
            : text;

        return new CompletionItem(text, insertText ?? text);
    }
}