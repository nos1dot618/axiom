using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor;
using Axiom.Editor.Completion;
using Axiom.Editor.Diagnostics;

namespace Axiom.Core.Services;

public sealed class LspSession(ILspService lspService) : IAsyncDisposable
{
    public ILspService LspService { get; } = lspService;
    private CompletionService? CompletionService { get; set; }
    public DiagnosticService? DiagnosticService { get; private set; }

    public async ValueTask DisposeAsync()
    {
        CompletionService?.Dispose();
        DiagnosticService?.Dispose();

        await LspService.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await LspService.InitializeAsync();

        ToggleFeatures();
    }

    public void ToggleFeatures()
    {
        var lspSettings = ServiceFactory.SettingsService.CurrentSettings.Lsp;

        if (lspSettings.EnableCodeCompletion)
        {
            CompletionService ??= new CompletionService(
                EditorContext.GetEditor().TextArea,
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
            DiagnosticService ??= new DiagnosticService(EditorContext.GetEditor());
        }
        else
        {
            DiagnosticService?.Dispose();
            DiagnosticService = null;
        }
    }

    private static async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        var lspService = ServiceFactory.LspSession.LspService;
        var fileService = ServiceFactory.FileService;
        if (fileService.DocumentMetadata == null) return [];

        DocumentPosition position = new(EditorContext.GetEditor().TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await lspService.GetCompletionsAsync(fileService.DocumentMetadata!, position, contextDto);
    }
}