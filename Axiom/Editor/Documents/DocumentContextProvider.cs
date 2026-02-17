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
        if (_currentDocumentUri is null) throw new KeyNotFoundException($"Document not found: {_currentDocumentUri}");

        var context = new DocumentContext(_currentDocumentUri, new DiagnosticService(EditorContext.GetEditor()!));

        if (!Documents.TryAdd(_currentDocumentUri, context))
            throw new InvalidOperationException($"Document already exists: {_currentDocumentUri}");
    }

    public static void Close()
    {
        if (_currentDocumentUri is not null) Documents.TryRemove(_currentDocumentUri, out _);
    }
}