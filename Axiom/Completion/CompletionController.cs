using System.Windows;
using System.Windows.Input;
using Axiom.UI;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Axiom.Completion;

public sealed class CompletionController
{
    /// <summary>
    /// Delay in milliseconds before making request for completions.
    /// </summary>
    private const int CompletionRequestDelay = 100;

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
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null || e.Text.Length <= 0 || char.IsLetterOrDigit(e.Text[0])) return;

        // TODO: Make a set of characters, upon encountering: we must request for new completions,
        //       instead of appending into the current completion buffer.
        if (_triggerCharacters.Contains(e.Text) || e.Text[0] == ' ') _completionWindow.Close();

        _completionWindow?.CompletionList.RequestInsertion(e);
    }

    private async void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        try
        {
            // Completion happens only if the entered character is on of the trigger characters.
            if (!_triggerCharacters.Contains(e.Text)) return;

            _completionWindow = new CompletionWindow(_textArea);

            await Task.Delay(CompletionRequestDelay);
            var completionItems = await _completionProvider(e.Text);

            var data = _completionWindow.CompletionList.CompletionData;
            data.Clear();
            foreach (var item in completionItems) data.Add(item);

            StyleCompletionWindow();
            _completionWindow.Show();

            _completionWindow.Closed += delegate { _completionWindow = null; };
        }
        catch (Exception ex)
        {
            MainWindow.HandleException(ex);
        }
    }

    private void StyleCompletionWindow()
    {
        if (_completionWindow == null) return;

        var listBox = _completionWindow.CompletionList.ListBox;

        _completionWindow.Style = (Style)Application.Current.FindResource("AxiomCompletionWindowStyle")!;
        listBox.Style = (Style)Application.Current.FindResource("AxiomCompletionListStyle")!;
        listBox.ItemContainerStyle = (Style)Application.Current.FindResource("AxiomCompletionItemStyle")!;

        _completionWindow.FontFamily = Stylesheet.FontFamily;
        _completionWindow.FontSize = Stylesheet.FontSize;
    }
}