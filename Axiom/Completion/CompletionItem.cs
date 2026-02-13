using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Completion;

public sealed class CompletionItem(string text, string insertText) : ICompletionData
{
    public ImageSource? Image => null;
    public string Text { get; } = text;
    public object Content => Text;
    public object Description => Text;
    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, insertText);
    }
}