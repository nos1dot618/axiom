namespace Axiom.Infrastructure.Lsp.Language;

public static class LspRegistry
{
    private static Dictionary<string, LspServerConfiguration> Servers { get; } = new();

    public static void Add(LspServerConfiguration configuration)
    {
        Servers[configuration.LanguageId] = configuration;
    }

    public static LspServerConfiguration? Get(string languageId)
    {
        Servers.TryGetValue(languageId, out var configuration);
        return configuration;
    }
}