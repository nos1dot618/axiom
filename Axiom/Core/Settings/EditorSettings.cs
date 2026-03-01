// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using Axiom.Core.Settings.Sections.Editor;
using Axiom.Core.Settings.Sections.Language;
using Axiom.Core.Settings.Sections.Lsp;
using Axiom.Core.Settings.Sections.Project;

namespace Axiom.Core.Settings;

public class EditorSettings
{
    public int ConfigVersion { get; set; } = 1;

    public EditorSection Editor { get; set; } = new();
    public ProjectSection Project { get; set; } = new();
    public LspSection Lsp { get; set; } = new();
    public LanguagesSection Language { get; set; } = new();
}