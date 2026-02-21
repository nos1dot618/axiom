namespace Axiom.Infrastructure.Lsp.Language;

public sealed class LspServerConfiguration(string languageId, string command, string arguments)
{
    public string LanguageId { get; } = languageId;
    public string Command { get; } = command;
    public string Arguments { get; } = arguments;
}