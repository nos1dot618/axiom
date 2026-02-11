using System.Windows;
using Axiom.Lsp;

namespace Axiom;

public partial class MainWindow : Window
{
    private LspClient _lspClient;

    public MainWindow()
    {
        InitializeComponent();

        Editor.Text = "";
        SetEditorOptions();

        _lspClient = new LspClient();

        Loaded += async (_, __) =>
        {
            await _lspClient.StartAsync(
                "../../../../node_modules/.bin/pyright-langserver.cmd",
                "--stdio"
            );
            await _lspClient.InitializeAsync();
        };
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _lspClient.ShutdownAsync();
        base.OnClosed(e);
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