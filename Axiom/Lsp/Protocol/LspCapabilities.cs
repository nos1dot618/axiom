namespace Axiom.Lsp.Protocol;

public sealed class LspCapabilities
{
    public bool SupportsCompletion { get; init; }
    public bool SupportsHover { get; init; }
    public bool SupportsFormatting { get; init; }
    public bool SupportsIncrementalSync { get; init; }
}