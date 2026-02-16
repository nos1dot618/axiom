using System.IO;
using System.Windows;
using System.Windows.Input;
using Axiom.Core.Completion;
using Axiom.Core.Documents;
using Axiom.Editor.Diagnostics;
using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI.Editor;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;

namespace Axiom.App;

public partial class MainWindow
{
    private readonly LspLanguageService? _lspService;
    private readonly DocumentManager _documentManager;

    private DocumentMetadata? _documentMetadata;

    public MainWindow()
    {
        InitializeComponent();

        LspServerConfiguration lspConfiguration = new(
            languageId: "python",
            command: "../../../../node_modules/.bin/pyright-langserver.cmd",
            arguments: "--stdio",
            rootPath: @"C:\Users\nosferatu\Downloads"
        );

        var diagnosticService = new DiagnosticService(Editor);

        _lspService = new LspLanguageService(lspConfiguration, diagnosticService);
        _documentManager = new DocumentManager(Editor);
        // ReSharper disable once ObjectCreationAsStatement

        EditorConfigurator.Configure(Editor);
        SetKeybindings();

        Editor.Text = "";

        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
        Editor.Document.Changed += OnDocumentChanged;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_lspService != null)
            {
                await _lspService.InitializeAsync();

                // ReSharper disable once ObjectCreationAsStatement
                new CompletionController(
                    Editor.TextArea,
                    _lspService!.Capabilities.CompletionTriggerCharacters,
                    CompletionProvider
                );
            }

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
            if (_lspService != null) await _lspService.DisposeAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async Task<IReadOnlyList<CompletionItem>> CompletionProvider(string? triggerCharacter)
    {
        if (_documentMetadata == null && _lspService == null) return [];

        DocumentPosition position = new(Editor.TextArea.Caret);
        var contextDto = new CompletionContextDto(triggerCharacter);
        return await _lspService!.GetCompletionsAsync(_documentMetadata!, position, contextDto);
    }

    // TODO: Must be moved to common namespace.
    public static void HandleException(Exception ex)
    {
        Console.Write(ex.StackTrace);
        MessageBox.Show($"Failed to start LSP:\n{ex.Message}");
    }

    private async Task OpenFileAsync(string filePath)
    {
        var text = await _documentManager.LoadFileAsync(filePath);

        if (_lspService != null) _documentMetadata = await _lspService.OpenDocumentAsync(filePath, "python", text);
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

            if (_lspService != null) await _lspService.SaveDocumentAsync(_documentMetadata);
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
            // Close tooltip if exists.
            if (Editor.ToolTip != null)
            {
                ((System.Windows.Controls.ToolTip)Editor.ToolTip).IsOpen = false;
                Editor.ToolTip = null;
            }

            if (_documentMetadata == null || _documentManager.SuppressChanges) return;

            var changeDto = _documentManager.CreateChange(e);

            if (_lspService != null) await _lspService.ChangeDocumentAsync(_documentMetadata, changeDto);
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