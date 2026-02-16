using Axiom.Core.Documents;

namespace Axiom.Core.Diagnostics;

public sealed class Diagnostic
{
    public DocumentPosition StartPosition { get; init; }
    public DocumentPosition EndPosition { get; init; }
    public string Message { get; init; }
    public DiagnosticSeverity Severity { get; init; }
    public string Code { get; init; }
    public string Source { get; init; }
    public string CodeDescriptionUrl { get; init; }
}