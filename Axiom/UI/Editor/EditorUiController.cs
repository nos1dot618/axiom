using System.Windows;
using System.Windows.Input;
using Axiom.Common;

namespace Axiom.UI.Editor;

public static class EditorUiController
{
    public static ICommand ExitCommand { get; } = new RelayCommand(_ => Exit());

    private static void Exit()
    {
        Application.Current.Shutdown();
    }
}