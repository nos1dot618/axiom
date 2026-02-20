using System.Collections.Concurrent;
using Axiom.Editor.Diagnostics;

namespace Axiom.Editor.Documents;

// TODO: Improve this ContextProvider.
public static class DocumentContextProvider
{
    private static string? _currentDocumentUri;
    private static readonly ConcurrentDictionary<string, DocumentContext> Documents = new();

    public static void SetCurrentDocument(string uri) => _currentDocumentUri = uri;

    public static DocumentContext? Get()
    {
        if (_currentDocumentUri is not null && Documents.TryGetValue(_currentDocumentUri, out var context))
            return context;
        return null;
    }

    public static void Create()
    {
        if (_currentDocumentUri is null) return;
        
        Console.WriteLine(_currentDocumentUri);

        var context = new DocumentContext(_currentDocumentUri, new DiagnosticService(EditorContext.GetEditor()));
        Documents.TryAdd(_currentDocumentUri, context);
    }

    public static void Close()
    {
        if (_currentDocumentUri is not null && Documents.TryRemove(_currentDocumentUri, out var context))
        {
            context.DiagnosticService.Dispose();
        }
    }

    public static void DisposeAllDiagnostics()
    {
        foreach (var context in Documents.Values) context.DiagnosticService.Dispose();
        // TODO: Clearing may not be the correct approach, once more things are added to the DocumentContext
        //       besides just DiagnosticService.
        Documents.Clear();
    }
}