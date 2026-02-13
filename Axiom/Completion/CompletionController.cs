using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Completion;

public sealed class CompletionController
{
    private readonly TextArea _textArea;
    private CompletionWindow? _completionWindow;

    public CompletionController(TextArea textArea)
    {
        _textArea = textArea;
        _textArea.TextEntering += OnTextEntering;
    }

    public void Show(IReadOnlyList<CompletionItem> completionItems)
    {
        if (_completionWindow != null) return;

        _completionWindow?.Close();

        _completionWindow = new CompletionWindow(_textArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var completionItem in completionItems)
        {
            data.Add(completionItem);
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => _completionWindow = null;
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null) return;

        if (e.Text.Length > 0 && char.IsLetterOrDigit(e.Text[0]))
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }
}