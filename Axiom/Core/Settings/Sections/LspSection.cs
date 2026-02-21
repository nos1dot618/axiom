namespace Axiom.Core.Settings.Sections;

public class LspSection
{
    public bool EnableLsp { get; set; } = true;
    public bool EnableCodeCompletion { get; set; } = true;
    public bool EnableDiagnostics { get; set; } = true;
}