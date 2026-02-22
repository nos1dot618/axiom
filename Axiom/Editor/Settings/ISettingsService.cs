using Axiom.Core.Settings;

namespace Axiom.Editor.Settings;

public interface ISettingsService
{
    EditorSettings CurrentSettings { get; }

    void Update(Action<EditorSettings> action);
}