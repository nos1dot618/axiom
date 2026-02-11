using System.Windows;
using Axiom.Lsp;

namespace Axiom;

public partial class MainWindow
{
    private readonly LspClient _lspClient = new();
    private bool _lspStarted;

    private const string ServerPath = "../../../../node_modules/.bin/pyright-langserver.cmd";

    public MainWindow()
    {
        InitializeComponent();

        Editor.Text = "";
        SetEditorOptions();

        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _lspClient.StartAsync(ServerPath, "--stdio");
            // rootUri is null, can be replaced with the root path of the project.
            await _lspClient.InitializeAsync();
            _lspStarted = true;
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

    private void HandleException(Exception ex)
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
}