using Axiom.Editor.Diagnostics;

namespace Axiom.Editor.Documents;

public sealed class DocumentContext(string uri, DiagnosticService diagnosticService)
{
    public string Uri => uri;
    public DiagnosticService DiagnosticService => diagnosticService;
}