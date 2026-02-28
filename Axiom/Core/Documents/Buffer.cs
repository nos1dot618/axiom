using Axiom.Infrastructure.Lsp.Language;
using Axiom.UI;

namespace Axiom.Core.Documents;

public class Buffer
{
    private static int _untitledCount = 1;

    public Buffer()
    {
        Path = $"untitled:Untitled-{_untitledCount++}";
        IsVirtual = true;
    }

    public string Path { get; private set; }
    public bool IsVirtual { get; private set; }
    public string? LanguageId { get; set; }

    public void Change(string path)
    {
        Path = new Uri(path).AbsolutePath;
        IsVirtual = false;

        var languageId = LanguageIdResolver.GetLanguageId(Path);
        // If extension mapping for the current file extension does not exist then do not lose the previous language ID.
        if (languageId != null) LanguageId = languageId;
        UiControllersRegistry.Language.Update();
    }
}