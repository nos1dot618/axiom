using System.Windows;
using System.Windows.Controls;
using Axiom.Editor;

namespace Axiom.UI.Themes;

public sealed class ThemeUiController(MenuItem themesMenuItem)
{
    public void Populate()
    {
        themesMenuItem.Items.Clear();

        foreach (var theme in ServicesRegistry.ThemeService.Themes)
        {
            var menuItem = new MenuItem
            {
                Header = theme.Name,
                IsCheckable = true,
                Tag = theme
            };
            menuItem.Click += OnClick;
            themesMenuItem.Items.Add(menuItem);
        }

        UpdateThemeCheckmarks();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: EditorTheme theme }) return;

        ServicesRegistry.ThemeService.SetTheme(theme);
        ThemeApplicator.Apply();
        UpdateThemeCheckmarks();
    }

    private void UpdateThemeCheckmarks()
    {
        foreach (MenuItem item in themesMenuItem.Items)
            if (item.Tag is EditorTheme theme)
                item.IsChecked = theme == ServicesRegistry.ThemeService.CurrentTheme;
    }
}