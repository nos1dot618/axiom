using System.IO;
using Tomlyn;

namespace Axiom.Core.Settings;

public class SettingsService : ISettingsService
{
    private readonly string _filepath;

    public EditorSettings CurrentSettings { get; private set; } = new();

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "Axiom");

        Directory.CreateDirectory(appDirectory);

        _filepath = Path.Combine(appDirectory, "settings.toml");
    }

    public void Load()
    {
        if (!File.Exists(_filepath))
        {
            CurrentSettings = new EditorSettings();
            Save(); // Create default configuration file.
            return;
        }

        var configText = File.ReadAllText(_filepath);
        CurrentSettings = Toml.ToModel<EditorSettings>(configText);
    }

    public void Save()
    {
        var configText = Toml.FromModel(CurrentSettings);
        File.WriteAllText(_filepath, configText);
    }
}