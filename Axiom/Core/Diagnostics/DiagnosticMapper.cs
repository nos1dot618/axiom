using System.Text.Json;
using Axiom.Core.Documents;

namespace Axiom.Core.Diagnostics;

public class DiagnosticMapper
{
    // TODO: The logic of this class is very similar to CompletionItemMapper,
    //       the logic can be combined through an interface maybe.
    public static IReadOnlyList<Diagnostic> Map(JsonElement result)
    {
        var itemsElement = ExtractItems(result);
        var diagnostics = new List<Diagnostic>();

        if (itemsElement.ValueKind != JsonValueKind.Array) return diagnostics;

        diagnostics.AddRange(itemsElement.EnumerateArray().Select(MapSingle));
        return diagnostics;
    }

    private static JsonElement ExtractItems(JsonElement result)
    {
        return result.ValueKind switch
        {
            JsonValueKind.Array => result,
            JsonValueKind.Object when result.TryGetProperty("diagnostics", out var items) => items,
            _ => default
        };
    }

    private static Diagnostic MapSingle(JsonElement item)
    {
        var range = item.GetProperty("range");
        var start = range.GetProperty("start");
        var end = range.GetProperty("end");

        var startPosition = new DocumentPosition(start.GetProperty("line").GetInt32(),
            start.GetProperty("character").GetInt32());
        var endPosition =
            new DocumentPosition(end.GetProperty("line").GetInt32(), end.GetProperty("character").GetInt32());

        var severity = DiagnosticSeverity.Info;
        if (item.TryGetProperty("severity", out var severityElement))
        {
            severity = severityElement.GetInt32().ToDiagnosticSeverity() ?? DiagnosticSeverity.Info;
        }

        return new Diagnostic
        {
            StartPosition = startPosition,
            EndPosition = endPosition,
            Message = item.GetProperty("message").GetString() ?? string.Empty,
            Severity = severity,
            Code = item.TryGetProperty("code", out var codeElement) ? codeElement.ToString() : string.Empty,
            Source = item.TryGetProperty("source", out var sourceElement)
                ? sourceElement.GetString() ?? string.Empty
                : string.Empty,
            CodeDescriptionUrl =
                item.TryGetProperty("codeDescription", out var codeDescriptionElement) &&
                codeDescriptionElement.TryGetProperty("href", out var hrefElement)
                    ? hrefElement.GetString() ?? string.Empty
                    : string.Empty
        };
    }
}