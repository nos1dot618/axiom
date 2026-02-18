using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public interface IEditorService
{
    Task OnLoadCallback();
    Task OnCloseCallback();
    Task OnDocumentChangeCallback(DocumentChangeEventArgs e);
}