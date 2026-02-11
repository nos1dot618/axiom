using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Lsp;

public class LspCompletionData : ICompletionData
{
    public LspCompletionData(string text, string insertText)
    {
        Text = text;
        _insertText = insertText;
    }

    private readonly string _insertText;

    public ImageSource? Image => null;
    public string Text { get; }
    public object Content => Text;
    public object Description => Text;
    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, _insertText);
    }
}