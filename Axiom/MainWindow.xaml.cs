using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Axiom.Lsp;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Axiom;

public partial class MainWindow
{
    private const string ServerPath = "../../../../node_modules/.bin/pyright-langserver.cmd";

    private readonly LspClient _lspClient = new();
    private bool _lspStarted;

    // AvalonEdit
    private CompletionWindow? _completionWindow;
    private int _documentVersion = 1;
    private string _currentFileUri = "file:///C:/Users/nosferatu/Downloads/test.py";

    public MainWindow()
    {
        InitializeComponent();

        Editor.Text = "";
        SetEditorOptions();
        SetKeybindings();

        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
        // Editor.TextArea.TextEntered += OnTextEntered;
        Editor.TextArea.TextEntering += OnTextEntering;
        Editor.TextChanged += OnEditorTextChanged;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _lspClient.StartAsync(ServerPath, "--stdio");
            await _lspClient.InitializeAsync(new Uri(@"C:\Users\nosferatu\Downloads").AbsoluteUri);
            _lspStarted = true;

            await OpenFileAsync(@"C:\Users\nosferatu\Downloads\test.py");
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async void OnClosedAsync(object? sender, EventArgs e)
    {
        try
        {
            if (_lspStarted)
            {
                await _lspClient.ShutdownAsync();
            }

            _lspClient.Dispose();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        if (!_lspStarted) return;

        if (char.IsLetterOrDigit(e.Text[0]) || e.Text == ".")
        {
            await RequestCompletionAsync();
        }
    }

    private async void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null) return;

        if (e.Text.Length > 0 && char.IsLetterOrDigit(e.Text[0]))
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private async Task RequestCompletionAsync()
    {
        var caret = Editor.TextArea.Caret;

        var result = await _lspClient.SendRequestAsync(LspClient.Method.TextCompletion, new
        {
            textDocument = new
            {
                uri = _currentFileUri
            },
            position = new
            {
                line = caret.Line - 1,
                character = caret.Column - 1
            }
        });

        if (result.ValueKind == JsonValueKind.Null) return;

        JsonElement items;
        if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var itemsEx))
            items = itemsEx;
        else if (result.ValueKind == JsonValueKind.Array) items = result;
        else return;

        RenderCompletions(items);
    }

    private void RenderCompletions(JsonElement items)
    {
        _completionWindow?.Close();

        _completionWindow = new CompletionWindow(Editor.TextArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var item in items.EnumerateArray())
        {
            var label = item.GetProperty("label").GetString() ?? "";
            var insertText = item.TryGetProperty("insertText", out var insertTextEx)
                ? insertTextEx.GetString()
                : label;

            data.Add(new LspCompletionData(label, insertText ?? label));
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, __) => _completionWindow = null;
    }

    private static void HandleException(Exception ex)
    {
        MessageBox.Show($"Failed to start LSP:\n{ex.Message}");
    }

    private void SetEditorOptions()
    {
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.IndentationSize = 4;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.HighlightCurrentLine = true;
        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowSpaces = false;
    }

    private async Task OpenFileAsync(string filePath)
    {
        Editor.Text = await File.ReadAllTextAsync(filePath);
        _currentFileUri = new Uri(filePath).AbsoluteUri;
        _documentVersion = 1;

        if (!_lspStarted) return;
        await _lspClient.SendDidOpen(_currentFileUri, "python", _documentVersion, Editor.Text);
    }

    private async void OnEditorTextChanged(object? sender, EventArgs e)
    {
        try
        {
            if (!_lspStarted) return;
            _documentVersion++;

            await _lspClient.SendDidChange(_currentFileUri, _documentVersion, Editor.Text);
            await RequestCompletionAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async void OnOpenExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*"
            };

            // TODO: Save file if any changes before switching to a different file.
            if (dialog.ShowDialog() == true)
            {
                await OpenFileAsync(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentFileUri)) return;

            var filePath = new Uri(_currentFileUri).LocalPath;
            await File.WriteAllTextAsync(filePath, Editor.Text);

            if (_lspStarted) await _lspClient.SendDidSave(_currentFileUri);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void SetKeybindings()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OnOpenExecuted));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, OnSaveExecuted));

        InputBindings.Add(new KeyBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
    }
}