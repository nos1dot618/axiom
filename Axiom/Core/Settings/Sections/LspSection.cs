using Axiom.Infrastructure.Lsp.Language;

namespace Axiom.Core.Settings.Sections;

public class LspSection
{
    public bool EnableLsp { get; set; } = true;
    public bool EnableCodeCompletion { get; set; } = true;
    public bool EnableDiagnostics { get; set; } = true;

    public LspRootMode DefaultRootMode { get; set; } = LspRootMode.Workspace;
    public string? FixedRootPath { get; set; }

    public List<LspServerSection> Servers { get; set; } = [];
}