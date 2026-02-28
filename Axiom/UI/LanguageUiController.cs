using System.Windows;
using System.Windows.Controls;
using Axiom.Editor;
using Axiom.Editor.Documents;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Axiom.UI;

// TODO: Create an interface for UiController.
public sealed class LanguageUiController(MenuItem languageMenuItem)
{
    private static readonly Dictionary<string, string> SyntaxHighlightingMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C#"] = "csharp"
    };

    public void Populate()
    {
        languageMenuItem.Items.Clear();

        // TODO: For now this is assumed that all supported languages have syntax highlighting.
        foreach (var highlightingDefinition in
                 HighlightingManager.Instance.HighlightingDefinitions.OrderBy(definition => definition.Name))
        {
            var menuItem = new MenuItem
            {
                Header = highlightingDefinition.Name,
                IsCheckable = true,
                Tag = highlightingDefinition
            };
            menuItem.Click += OnClick;
            languageMenuItem.Items.Add(menuItem);
        }

        Update();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: IHighlightingDefinition highlightingDefinition }) return;

        var languageId = ResolveLanguageId(highlightingDefinition.Name);
        if (languageId == null) return;
        ServicesRegistry.EditorService.SetLanguage(languageId);
        Update();
    }

    public void Update()
    {
        foreach (MenuItem item in languageMenuItem.Items)
        {
            if (item.Tag is not IHighlightingDefinition highlightingDefinition) continue;
            var languageId = ResolveLanguageId(highlightingDefinition.Name);
            item.IsChecked = languageId == FileService.CurrentBuffer.LanguageId;
        }
    }

    private static string? ResolveLanguageId(string syntaxHighlightingName)
    {
        return string.IsNullOrWhiteSpace(syntaxHighlightingName)
            ? null
            : SyntaxHighlightingMap.GetValueOrDefault(syntaxHighlightingName, syntaxHighlightingName.ToLower());
    }
}