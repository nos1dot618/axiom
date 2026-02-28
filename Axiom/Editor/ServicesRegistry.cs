using Axiom.Editor.Build;
using Axiom.Editor.Documents;
using Axiom.Editor.Lsp;
using Axiom.Editor.Settings;
using Axiom.Editor.Themes;

namespace Axiom.Editor;

public static class ServicesRegistry
{
    private static DocumentManager? _documentManager;
    private static IFileService? _fileService;
    private static IEditorService? _editorService;
    private static ISettingsService? _settingsService;
    private static LspSession? _lspSession;
    private static IThemeService? _themeService;
    private static IRunService? _runService;

    public static LspSession LspSession
    {
        get => _lspSession ??= new LspSession(new NoOpLspService());
        set => _lspSession = value;
    }

    public static IFileService FileService
    {
        get => _fileService ??= new FileService();
        set => _fileService = value;
    }

    public static ISettingsService SettingsService
    {
        get => _settingsService ??= new SettingsService();
        set => _settingsService = value;
    }

    public static DocumentManager DocumentManager => _documentManager ??= new DocumentManager();
    public static IEditorService EditorService => _editorService ??= new EditorService();
    public static IThemeService ThemeService => _themeService ??= new ThemeService();
    public static IRunService RunService => _runService ??= new RunService();
}