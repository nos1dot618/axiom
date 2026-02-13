using System.Text.Json;

namespace Axiom.Completion;

public static class CompletionItemMapper
{
    public static IReadOnlyList<CompletionItem> Map(JsonElement result)
    {
        var itemsElement = ExtractItems(result);
        var completionItems = new List<CompletionItem>();

        if (itemsElement.ValueKind != JsonValueKind.Array) return completionItems;

        completionItems.AddRange(itemsElement.EnumerateArray().Select(MapSingle));
        return completionItems;
    }

    private static JsonElement ExtractItems(JsonElement result)
    {
        return result.ValueKind switch
        {
            JsonValueKind.Array => result,
            JsonValueKind.Object when result.TryGetProperty("items", out var items) => items,
            _ => default
        };
    }

    private static CompletionItem MapSingle(JsonElement item)
    {
        var text = item.GetProperty("label").GetString() ?? "";
        var insertText = item.TryGetProperty("insertText", out var insertTextEx)
            ? insertTextEx.GetString()
            : text;

        return new CompletionItem(text, insertText ?? text);
    }
}