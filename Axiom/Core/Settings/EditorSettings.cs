using Axiom.Core.Settings.Sections;

namespace Axiom.Core.Settings;

public class EditorSettings
{
    public int ConfigVersion { get; set; } = 1;

    public EditorSection Editor { get; set; } = new();
    public LspSection Lsp { get; set; } = new();
}