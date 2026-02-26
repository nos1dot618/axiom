using System.Windows;
using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;

namespace Axiom.UI.Documents;

public static class FileUiController
{
    public static ICommand RunFileCommand { get; } = new RelayCommand(_ => Run());

    private static void Run()
    {
        var command = $"python {ServicesRegistry.FileService.CurrentDocumentPath}";
        Console.WriteLine(command);
        ServicesRegistry.RunService.Run(command);
    }
}