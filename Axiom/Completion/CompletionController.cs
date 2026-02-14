using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Axiom.UI;
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
    private CancellationTokenSource? _cts;

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
        {
            _completionWindow?.Close();
            _completionWindow = null;
            return;
        }

        if (_completionWindow == null)
        {
            _completionWindow = new CompletionWindow(_textArea)
            {
                StartOffset = GetCompletionStartOffset()
            };

            StyleCompletionWindow();

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

            if (_cts != null) await _cts.CancelAsync();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Task.Delay(CompletionRequestDelay, token);
            var completionItems = await _completionProvider(e.Text);

            if (!token.IsCancellationRequested) Show(completionItems);
        }
        catch (TaskCanceledException)
        {
            // expected
        }
        catch (Exception ex)
        {
            MainWindow.HandleException(ex);
        }
    }

    private void StyleCompletionWindow()
    {
        if (_completionWindow == null) return;

        // Window
        _completionWindow.WindowStyle = WindowStyle.None;
        _completionWindow.ResizeMode = ResizeMode.NoResize;
        _completionWindow.BorderThickness = new Thickness(1);
        _completionWindow.BorderBrush = new SolidColorBrush(Color.FromRgb(90, 90, 95));

        _completionWindow.AllowsTransparency = false;

        _completionWindow.Padding = new Thickness(0);

        _completionWindow.SizeToContent = SizeToContent.WidthAndHeight;

        var listBox = _completionWindow.CompletionList.ListBox;

        listBox.BorderThickness = new Thickness(0);

        var popupBackground = new SolidColorBrush(Color.FromRgb(50, 50, 52)); // lighter than editor
        var popupBorder = new SolidColorBrush(Color.FromRgb(80, 80, 85));

        _completionWindow.Background = popupBackground;
        _completionWindow.BorderBrush = popupBorder;

        listBox.Background = popupBackground;

        listBox.ItemContainerStyle = CreateItemContainerStyle();

        listBox.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        listBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);

        _completionWindow.FontFamily = Stylesheet.FontFamily;
        _completionWindow.FontSize = Stylesheet.FontSize;
    }

    private Style CreateItemContainerStyle()
    {
        var style = new Style(typeof(ListBoxItem));

        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 5, 10, 5)));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

        // Hover
        var hoverTrigger = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };
        hoverTrigger.Setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(50, 50, 52))));
        style.Triggers.Add(hoverTrigger);

        // Selected
        var selectedTrigger = new Trigger
        {
            Property = ListBoxItem.IsSelectedProperty,
            Value = true
        };
        selectedTrigger.Setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(0, 122, 204))));
        selectedTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
        style.Triggers.Add(selectedTrigger);

        return style;
    }
}