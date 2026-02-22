using System.IO;
using Axiom.Core.Settings;
using Axiom.Infrastructure.Lsp.Language;
using Tomlyn;

namespace Axiom.Editor.Settings;

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

        foreach (var configuration in CurrentSettings.Lsp.Servers)
            LspRegistry.Add(new LspServerConfiguration(configuration.LanguageId, configuration.Command,
                configuration.Arguments));
    }

    public EditorSettings CurrentSettings { get; }

    public void Update(Action<EditorSettings> action)
    {
        action(CurrentSettings);
        Save();
    }

    private static string InitializeFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "Axiom");

        Directory.CreateDirectory(appDirectory);

        return Path.Combine(appDirectory, "settings.toml");
    }

    private void Save()
    {
        var configText = Toml.FromModel(CurrentSettings);
        File.WriteAllText(FilePath, configText);
    }
}