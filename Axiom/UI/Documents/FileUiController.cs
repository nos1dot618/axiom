using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;

namespace Axiom.UI.Documents;

public static class FileUiController
{
    public static ICommand RunFileCommand { get; } = new RelayCommand(_ => Run());
    public static ICommand OpenFileCommand { get; } = new RelayCommand(_ => Open());

    private static void Run()
    {
        var command = $"python {ServicesRegistry.FileService.CurrentDocumentPath}";
        Console.WriteLine(command);
        ServicesRegistry.RunService.Run(command);
    }

    private static void Open()
    {
        AsyncCommand.Execute(ServicesRegistry.FileService.OpenFileDialogAsync);
    }
}