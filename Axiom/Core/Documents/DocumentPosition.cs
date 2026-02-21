using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Core.Documents;

public sealed record DocumentPosition(int Row, int Column)
{
    public DocumentPosition(TextLocation location) : this(location.Line - 1, location.Column - 1)
    {
    }

    public DocumentPosition(Caret caret) : this(caret.Line - 1, caret.Column - 1)
    {
    }

    public object ToDto()
    {
        return new
        {
            line = Row,
            character = Column
        };
    }
}