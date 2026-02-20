using Axiom.Core.Settings;
using Axiom.Editor.Documents;

namespace Axiom.Core.Services;

public static class ServiceFactory
{
    private static DocumentManager? _documentManager;
    private static IFileService? _fileService;
    private static IEditorService? _editorService;
    private static ISettingsService? _settingsService;

    public static void Configure(DocumentManager documentManager, ILspService lspLanguageService)
    {
        _documentManager = documentManager;
        LspService = lspLanguageService;

        _settingsService = new SettingsService();
        _settingsService.Load();
    }

    private static DocumentManager DocumentManager =>
        _documentManager ?? throw new InvalidOperationException("Services not configured");

    public static ILspService LspService { get; set; } = new NoOpLspService();

    public static IFileService FileService => _fileService ??= new FileService(DocumentManager, LspService);

    public static IEditorService EditorService =>
        _editorService ??= new EditorService(DocumentManager, FileService);

    public static ISettingsService SettingsService =>
        _settingsService ?? throw new InvalidOperationException("Services not configured");
}