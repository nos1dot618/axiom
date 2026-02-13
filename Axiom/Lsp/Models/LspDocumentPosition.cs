using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Lsp.Models;

public sealed record LspDocumentPosition(int Row, int Column)
{
    public LspDocumentPosition(TextLocation location) : this(location.Line - 1, location.Column - 1)
    {
    }

    public object ToDto() => new
    {
        line = Row,
        character = Column
    };
}