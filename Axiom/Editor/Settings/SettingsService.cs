using System.IO;
using Axiom.Core.Settings;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Language;
using Tomlyn;

namespace Axiom.Editor.Settings;

public sealed class SettingsService : ISettingsService
{
    private const string ConfigurationFile = "settings.toml";
    private static string _filePath = InitializeFilePath();

    public SettingsService()
    {
        _filePath = InitializeFilePath();
        if (!File.Exists(_filePath))
        {
            CurrentSettings = new EditorSettings();
            Save(); // Create default configuration file.
            return;
        }

        var options = new TomlModelOptions
        {
            IgnoreMissingProperties = true
        };

        var configText = File.ReadAllText(_filePath);
        CurrentSettings = Toml.ToModel<EditorSettings>(configText, options: options);

        foreach (var configuration in CurrentSettings.Lsp.Servers)
            LspRegistry.Add(new LspServerConfiguration(configuration.LanguageId, configuration.Command,
                configuration.Arguments));
    }

    public static DirectoryInfo? ProjectPath => new FileInfo(_filePath).Directory?.Parent;

    public EditorSettings CurrentSettings { get; }

    public void Update(Action<EditorSettings> action)
    {
        action(CurrentSettings);
        Save();
    }

    private static string InitializeFilePath()
    {
        // If user has a file opened, then search for the first project specific configuration.
        if (!FileService.CurrentBuffer.IsVirtual)
        {
            var directory = new FileInfo(FileService.CurrentBuffer.Path).Directory;
            while (directory != null)
            {
                var settingsPath = Path.Combine(directory.FullName, ".axiom", ConfigurationFile);
                if (File.Exists(settingsPath)) return settingsPath;
                directory = directory.Parent;
            }
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "Axiom");

        Directory.CreateDirectory(appDirectory);

        return Path.Combine(appDirectory, ConfigurationFile);
    }

    private void Save()
    {
        var configText = Toml.FromModel(CurrentSettings);
        File.WriteAllText(_filePath, configText);
    }
}