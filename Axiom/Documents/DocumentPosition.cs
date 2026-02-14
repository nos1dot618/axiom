using System.Windows.Forms.VisualStyles;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Documents;

public sealed record DocumentPosition(int Row, int Column)
{
    public DocumentPosition(TextLocation location) : this(location.Line - 1, location.Column - 1)
    {
    }

    public DocumentPosition(Caret caret) : this(caret.Line - 1, caret.Column - 1)
    {
    }

    public object ToDto() => new
    {
        line = Row,
        character = Column
    };
}