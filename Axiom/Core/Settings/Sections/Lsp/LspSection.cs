// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

using Axiom.Infrastructure.Lsp.Language;

namespace Axiom.Core.Settings.Sections.Lsp;

public class LspSection
{
    public bool EnableLsp { get; set; } = true;
    public bool EnableCodeCompletion { get; set; } = true;
    public bool EnableDiagnostics { get; set; } = true;

    // TODO: This can be different for each language.
    public LspRootMode DefaultRootMode { get; set; } = LspRootMode.Workspace;
    public string? FixedRootPath { get; set; } = "";

    public List<LspServerSection> Servers { get; set; } = [];
}