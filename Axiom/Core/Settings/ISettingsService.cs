namespace Axiom.Core.Settings;

public interface ISettingsService
{
    EditorSettings CurrentSettings { get; }

    void Update(Action<EditorSettings> action);
}