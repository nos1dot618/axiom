using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;
using Axiom.Editor.Documents;

namespace Axiom.UI.Documents;

public static class FileUiController
{
    public static ICommand RunFileCommand { get; } = new RelayCommand(_ => Run());
    public static ICommand OpenFileCommand { get; } = new RelayCommand(_ => Open());
    public static ICommand NewFileCommand { get; } = new RelayCommand(_ => New());

    private static void Run()
    {
        // TODO: Assert that the file is non virtual.
        // TODO: User defined Run Configuration.
        var command = $"python {FileService.CurrentBuffer.Path}";
        ServicesRegistry.RunService.Run(command);
    }

    private static void Open()
    {
        AsyncCommand.Execute(ServicesRegistry.FileService.OpenFileDialogAsync);
    }

    private static void New()
    {
        AsyncCommand.Execute(ServicesRegistry.FileService.NewDocumentAsync);
    }
}