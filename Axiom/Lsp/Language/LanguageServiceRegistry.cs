namespace Axiom.Lsp.Language;

public sealed class LanguageServiceRegistry
{
    private readonly Dictionary<string, LspLanguageService> _services = new();

    public void Register(LspLanguageService service) => _services.Add(service.LanguageId, service);

    public LspLanguageService? Get(string languageId) => _services[languageId];
}