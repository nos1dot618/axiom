// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Axiom.Core.Settings.Sections.Project;

public class ProjectSection
{
    public bool BuildBeforeRun { get; set; } = true;
    public string BuildFormat { get; set; } = "echo \"Build Project command is not configured.\"";
    public string RunFormat { get; set; } = "echo \"Run Project command is not configured.\"";
    public string? BuildLogPath { get; set; } = null;
}