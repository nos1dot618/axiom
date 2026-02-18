using Axiom.Core.Settings;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;

namespace Axiom.Core.Services;

public static class ServiceFactory
{
    private static DocumentManager? _documentManager;
    private static IFileService? _fileService;
    private static IEditorService? _editorService;
    private static ISettingsService? _settingsService;

    public static void Configure(DocumentManager documentManager, LspLanguageService? lspLanguageService)
    {
        _documentManager = documentManager;
        LspLanguageService = lspLanguageService;

        _settingsService = new SettingsService();
        _settingsService.Load();
    }

    private static DocumentManager DocumentManager =>
        _documentManager ?? throw new InvalidOperationException("Services not configured");

    private static LspLanguageService? LspLanguageService { get; set; }

    public static IFileService FileService => _fileService ??= new FileService(DocumentManager, LspLanguageService);

    public static IEditorService EditorService =>
        _editorService ??= new EditorService(DocumentManager, LspLanguageService, FileService);

    public static ISettingsService SettingsService =>
        _settingsService ?? throw new InvalidOperationException("Services not configured");
}