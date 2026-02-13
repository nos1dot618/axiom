using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Axiom.Core.Completion;
using Axiom.Lsp.Documents;
using Axiom.Lsp.Language;
using Axiom.Lsp.Models;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;

namespace Axiom;

public partial class MainWindow
{
    private readonly LspLanguageService _lspService;
    private bool _lspStarted;

    // AvalonEdit
    private CompletionWindow? _completionWindow;
    private LspDocumentMetadata? _documentMetadata;

    // Suppress on document change callback during loading the document.
    private bool _suppressChanges;

    public MainWindow()
    {
        InitializeComponent();

        LspServerConfiguration lspConfiguration = new("python", "../../../../node_modules/.bin/pyright-langserver.cmd",
            "--stdio",
            @"C:\Users\nosferatu\Downloads");
        _lspService = new LspLanguageService(lspConfiguration);

        Editor.Text = "";
        SetEditorOptions();
        SetKeybindings();

        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
        // Editor.TextArea.TextEntered += OnTextEntered;
        Editor.TextArea.TextEntering += OnTextEntering;
        // Editor.TextChanged += OnEditorTextChanged;
        Editor.Document.Changed += OnDocumentChanged;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _lspService.InitializeAsync();
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
            await _lspService.DisposeAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null) return;

        if (e.Text.Length > 0 && char.IsLetterOrDigit(e.Text[0]))
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private async Task RequestCompletionAsync()
    {
        if (_documentMetadata == null) return;

        var caret = Editor.TextArea.Caret;
        LspDocumentPosition position = new(caret.Line - 1, caret.Column - 1);

        var completionItems = await _lspService.GetCompletionsAsync(_documentMetadata, position);

        RenderCompletions(completionItems);
    }

    private void RenderCompletions(IReadOnlyList<CompletionItem> completionItems)
    {
        _completionWindow?.Close();

        _completionWindow = new CompletionWindow(Editor.TextArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var completionItem in completionItems)
        {
            data.Add(completionItem);
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => _completionWindow = null;
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
        Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
    }

    private async Task OpenFileAsync(string filePath)
    {
        _suppressChanges = true;
        Editor.Text = await File.ReadAllTextAsync(filePath);
        _suppressChanges = false;

        if (!_lspStarted) return;
        _documentMetadata = await _lspService.OpenDocumentAsync(filePath, "python", Editor.Text);
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
            if (_documentMetadata == null || string.IsNullOrEmpty(_documentMetadata.Uri)) return;

            var filePath = new Uri(_documentMetadata.Uri).LocalPath;
            await File.WriteAllTextAsync(filePath, Editor.Text);

            if (_lspStarted) await _lspService.SaveDocumentAsync(_documentMetadata);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async void OnDocumentChanged(object? sender, DocumentChangeEventArgs e)
    {
        try
        {
            if (_documentMetadata == null || !_lspStarted || _suppressChanges) return;

            var document = Editor.Document;
            var startPosition = new LspDocumentPosition(document.GetLocation(e.Offset));
            var endPosition = new LspDocumentPosition(document.GetLocation(e.Offset + e.RemovalLength));
            var changeDto = new LspDocumentChangeDto(startPosition, endPosition, e.InsertedText.Text ?? string.Empty);

            await _lspService.ChangeDocumentAsync(_documentMetadata, changeDto);
            await RequestCompletionAsync();
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

    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}