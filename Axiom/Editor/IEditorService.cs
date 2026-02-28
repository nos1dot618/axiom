using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor;

public interface IEditorService
{
    Task OnLoadCallback();
    Task OnCloseCallback();
    Task OnDocumentChangeCallback(DocumentChangeEventArgs e);

    Task SetLanguage(string languageId);
    Task ToggleLsp();
}