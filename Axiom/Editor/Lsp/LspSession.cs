using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor.Completion;
using Axiom.Editor.Diagnostics;
using Axiom.Editor.Documents;

namespace Axiom.Editor.Lsp;

public sealed class LspSession : IAsyncDisposable
{
    public LspSession(ILspService lspService)
    {
        LspService = lspService;
        LanguageId = lspService.LanguageId;
        EffectiveLanguageId = lspService.LanguageId ?? EffectiveLanguageId;
    }

    public ILspService LspService { get; }
    private CompletionService? CompletionService { get; set; }
    public DiagnosticService? DiagnosticService { get; private set; }

    public static string? LanguageId { get; set; }

    /// <summary>
    ///     Last valid language ID. Used for acquiring the LSP configuration after re-enbling the LSP service.
    /// </summary>
    public static string? EffectiveLanguageId { get; private set; }

    public async ValueTask DisposeAsync()
    {
        CompletionService?.Dispose();
        DiagnosticService?.Dispose();

        await LspService.DisposeAsync();
    }

    private async Task InitializeAsync()
    {
        await LspService.InitializeAsync();

        ToggleFeatures();
    }

    public void ToggleFeatures()
    {
        var lspSettings = ServicesRegistry.SettingsService.CurrentSettings.Lsp;

        if (lspSettings.EnableCodeCompletion)
        {
            CompletionService ??= new CompletionService(
                EditorService.Editor.TextArea,
                LspService.Capabilities.CompletionTriggerCharacters,
                CompletionProvider
            );
        }
        else
        {
            CompletionService?.Dispose();
            CompletionService = null;
        }

        if (lspSettings.EnableDiagnostics)
        {
            DiagnosticService ??= new DiagnosticService(EditorService.Editor);
        }
        else
        {
            DiagnosticService?.Dispose();
            DiagnosticService = null;
        }
    }

    public static async Task Reload(ILspService lspService)
    {
        await ServicesRegistry.LspSession.DisposeAsync();

        ServicesRegistry.LspSession = new LspSession(lspService);
        await ServicesRegistry.LspSession.InitializeAsync();

        if (!FileService.CurrentDocumentAddress.IsVirtual)
        {
            // Reloading the LspService, thus makes sense to reset DocumentMetadata inside FileService as well.
            ServicesRegistry.FileService = new FileService();
            await ServicesRegistry.FileService.OpenDocumentAsync(FileService.CurrentDocumentAddress.Path,
                EditorService.Editor.Text);
        }
    }

    private static async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        var lspService = ServicesRegistry.LspSession.LspService;
        var fileService = ServicesRegistry.FileService;
        if (fileService.DocumentMetadata == null) return [];

        DocumentPosition position = new(EditorService.Editor.TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await lspService.GetCompletionsAsync(fileService.DocumentMetadata!, position, contextDto);
    }
}