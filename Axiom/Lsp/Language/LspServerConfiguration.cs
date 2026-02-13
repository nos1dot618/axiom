namespace Axiom.Lsp.Language;

public sealed class LspServerConfiguration(string languageId, string command, string arguments, string rootPath)
{
    public string LanguageId { get; } = languageId;
    public string Command { get; } = command;
    public string Arguments { get; } = arguments;
    public string RootPath { get; } = rootPath;
}