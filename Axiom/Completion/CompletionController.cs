using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Completion;

public sealed class CompletionController
{
    /// <summary>
    /// Delay in milliseconds before making request for completions.
    /// </summary>
    private const int CompletionRequestDelay = 150;

    private readonly TextArea _textArea;
    private readonly HashSet<string> _triggerCharacters;
    private readonly Func<string?, Task<IReadOnlyList<CompletionItem>>> _completionProvider;

    private CompletionWindow? _completionWindow;

    public CompletionController(TextArea textArea, IReadOnlyList<string> triggerCharacters,
        Func<string?, Task<IReadOnlyList<CompletionItem>>> completionProvider)
    {
        _textArea = textArea;
        _triggerCharacters = triggerCharacters.ToHashSet();
        _completionProvider = completionProvider;

        _textArea.TextEntering += OnTextEntering;
        _textArea.TextEntered += OnTextEntered;
        _textArea.PreviewKeyDown += OnPreviewKeyDown;
    }

    private void Show(IReadOnlyList<CompletionItem> completionItems)
    {
        if (completionItems.Count == 0)
            return;

        if (_completionWindow == null)
        {
            _completionWindow = new CompletionWindow(_textArea) { StartOffset = GetCompletionStartOffset() };
            _completionWindow.Closed += (_, _) => _completionWindow = null;

            _completionWindow.Show();
        }

        var data = _completionWindow.CompletionList.CompletionData;
        data.Clear();

        foreach (var item in completionItems) data.Add(item);
    }

    private int GetCompletionStartOffset()
    {
        var offset = _textArea.Caret.Offset;
        var document = _textArea.Document;

        while (offset > 0)
        {
            var character = document.GetCharAt(offset - 1);
            if (!char.IsLetterOrDigit(character) && character != '_') break;

            offset--;
        }

        return offset;
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (_completionWindow == null || e.Key != Key.Back) return;

        if (_textArea.Caret.Offset <= _completionWindow.StartOffset) _completionWindow.Close();
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null) return;

        if (e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]))
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private async void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        try
        {
            var isTrigger = e.Text.Length > 0 && _triggerCharacters.Contains(e.Text);
            if (!isTrigger && _completionWindow == null) return;

            await Task.Delay(CompletionRequestDelay);

            var completionItems = await _completionProvider(e.Text);
            Show(completionItems);
        }
        catch (Exception ex)
        {
            MainWindow.HandleException(ex);
        }
    }
}