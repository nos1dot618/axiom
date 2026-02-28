// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Axiom.Core.Settings.Sections.Language;

public class LanguageSection
{
    public string LanguageId { get; set; } = null!;
    public string RunFormat { get; set; } = "echo \"Run command is not configured.\"";
}