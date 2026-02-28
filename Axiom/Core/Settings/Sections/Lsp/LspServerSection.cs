// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Axiom.Core.Settings.Sections.Lsp;

public class LspServerSection
{
    public string LanguageId { get; set; } = null!;
    public string Command { get; set; } = null!;
    public string Arguments { get; set; } = "";
    public List<string> FileExtensions { get; set; } = [];
}