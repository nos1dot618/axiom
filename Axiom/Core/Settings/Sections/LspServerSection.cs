namespace Axiom.Core.Settings.Sections;

public class LspServerSection
{
    public string LanguageId { get; set; } = null!;
    public string Command { get; set; } = null!;
    public string Arguments { get; set; } = "";
    public List<string> FileExtensions { get; set; } = [];
}