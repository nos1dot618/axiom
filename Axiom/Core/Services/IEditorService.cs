using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Core.Services;

public interface IEditorService
{
    bool IsLspEnabled { get; }

    Task OnLoadCallback();
    Task OnCloseCallback();
    Task OnDocumentChangeCallback(DocumentChangeEventArgs e);

    Task ToggleLsp();
}