using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Axiom;

public partial class MainWindow : Window
{
    private string _textBuffer = "Axiom Editor\n\nStart typing...";
    private int _caretIndex = 0;

    public MainWindow()
    {
        InitializeComponent();

        EditorCanvas.Focus();
        EditorCanvas.KeyDown += OnKeyDown;
        EditorCanvas.TextInput += OnTextInput;
        EditorCanvas.Loaded += (_, _) => RenderText();
    }

    private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
    {
        bool changed = false;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (keyEventArgs.Key)
        {
            case Key.Back when _caretIndex > 0:
            {
                _textBuffer = _textBuffer.Remove(_caretIndex - 1, 1);
                _caretIndex--;
                changed = true;
                break;
            }
            case Key.Enter:
            {
                _textBuffer += "\n";
                changed = true;
                break;
            }
        }

        if (changed) RenderText();
    }

    private void OnTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs)
    {
        _textBuffer = _textBuffer.Insert(_caretIndex, textCompositionEventArgs.Text);
        _caretIndex += textCompositionEventArgs.Text.Length;
        RenderText();
    }

    private void RenderText()
    {
        EditorCanvas.Children.Clear();
        var lines = _textBuffer.Split("\n");
        for (int index = 0; index < lines.Length; index++)
        {
            var textBlock = new TextBlock
            {
                Text = lines[index],
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14
            };

            Canvas.SetLeft(textBlock, 10);
            Canvas.SetTop(textBlock, 10 + index * 20);
            EditorCanvas.Children.Add(textBlock);
        }
    }
}