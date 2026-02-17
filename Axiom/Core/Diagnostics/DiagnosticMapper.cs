using System.Text.Json;
using Axiom.Core.Documents;

namespace Axiom.Core.Diagnostics;

public sealed class DiagnosticMapper : IServiceMapper<Diagnostic>
{
    public string ResultSetName => "diagnostics";

    public Diagnostic MapSingle(JsonElement item)
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