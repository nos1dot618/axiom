namespace Axiom.Lsp.Documents;

public sealed class LspDocumentMetadata(string uri, string languageId)
{
    public string Uri { get; } = uri;
    public string LanguageId { get; } = languageId;
    public int Version { get; private set; } = 1;

    public void IncrementVersion()
    {
        Version++;
    }
}