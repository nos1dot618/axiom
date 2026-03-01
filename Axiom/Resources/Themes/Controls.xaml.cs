using System.Windows;

namespace Axiom.Resources.Themes;

public partial class Controls
{
    private void CloseWindow_Event(object sender, RoutedEventArgs e)
    {
        if (e.Source != null)
            CloseWind(Window.GetWindow((FrameworkElement)e.Source));
    }

    private void AutoMinimize_Event(object sender, RoutedEventArgs e)
    {
        if (e.Source != null)
            MaximizeRestore(Window.GetWindow((FrameworkElement)e.Source));
    }

    private void Minimize_Event(object sender, RoutedEventArgs e)
    {
        if (e.Source != null)
            MinimizeWind(Window.GetWindow((FrameworkElement)e.Source));
    }

    public void CloseWind(Window window)
    {
        window?.Close();
    }

    public void MaximizeRestore(Window window)
    {
        if (window == null)
            return;
        switch (window.WindowState)
        {
            case WindowState.Normal:
                window.WindowState = WindowState.Maximized;
                break;
            case WindowState.Minimized:
            case WindowState.Maximized:
                window.WindowState = WindowState.Normal;
                break;
        }
    }

    public void MinimizeWind(Window window)
    {
        if (window != null)
            window.WindowState = WindowState.Minimized;
    }
}