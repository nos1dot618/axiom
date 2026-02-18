namespace Axiom.Core.Settings;

public interface ISettingsService
{
    EditorSettings CurrentSettings { get; }

    void Load();
    void Save();
}