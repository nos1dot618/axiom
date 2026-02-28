using System.Windows.Input;
using Axiom.Common;
using Axiom.Editor;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Logging;

namespace Axiom.UI.Documents;

public static class FileUiController
{
    public static ICommand RunFileCommand { get; } = new RelayCommand(_ => Run());
    public static ICommand OpenFileCommand { get; } = new RelayCommand(_ => Open());
    public static ICommand NewFileCommand { get; } = new RelayCommand(_ => New());

    private static void Run()
    {
        if (FileService.CurrentBuffer.IsVirtual)
        {
            ErrorHandler.DisplayMessage("Cannot run temporary buffers. Please save first.");
            return;
        }

        var languageSection =
            ServicesRegistry.SettingsService.CurrentSettings.Language.Languages.FirstOrDefault(section =>
                string.Equals(section.LanguageId, FileService.CurrentBuffer.LanguageId,
                    StringComparison.OrdinalIgnoreCase));


        if (languageSection == null)
        {
            ErrorHandler.DisplayMessage("Run configuration not found for the current language. " +
                                        "Please provide a run configuration inside the configuration file.");
            return;
        }

        ServicesRegistry.RunService.Run(languageSection.RunFormat);
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