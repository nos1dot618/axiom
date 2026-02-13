using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Documents;

public sealed record DocumentPosition(int Row, int Column)
{
    public DocumentPosition(TextLocation location) : this(location.Line - 1, location.Column - 1)
    {
    }

    public object ToDto() => new
    {
        line = Row,
        character = Column
    };
}