using System.IO;
using Tomlyn;

namespace Axiom.Core.Settings;

public sealed class SettingsService : ISettingsService
{
    private static readonly string FilePath = InitializeFilePath();

    public SettingsService()
    {
        if (!File.Exists(FilePath))
        {
            CurrentSettings = new EditorSettings();
            Save(); // Create default configuration file.
            return;
        }

        var configText = File.ReadAllText(FilePath);
        CurrentSettings = Toml.ToModel<EditorSettings>(configText);
    }

    public EditorSettings CurrentSettings { get; }

    private static string InitializeFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "Axiom");

        Directory.CreateDirectory(appDirectory);

        return Path.Combine(appDirectory, "settings.toml");
    }

    public void Save()
    {
        var configText = Toml.FromModel(CurrentSettings);
        File.WriteAllText(FilePath, configText);
    }
}