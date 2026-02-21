using System.IO;
using Axiom.Core.Documents;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor.Documents;

public sealed class DocumentManager
{
    public bool SuppressChanges { get; private set; }

    // TODO: fix this madness, we are always using LocalPath, there is no need to store this as an URI.
    public static string? CurrentDocumentUri { get; private set; }

    public async Task<string> LoadFileAsync(string filepath)
    {
        SuppressChanges = true;
        var text = await File.ReadAllTextAsync(filepath);
        EditorContext.GetEditor().Text = text;
        SuppressChanges = false;

        CurrentDocumentUri = new Uri(filepath).AbsoluteUri;
        return text;
    }

    public static void CloseFile()
    {
    }

    public async Task SaveFileAsync(string filePath)
    {
        await File.WriteAllTextAsync(filePath, EditorContext.GetEditor().Text);
    }

    public static DocumentChangeDto CreateChange(DocumentChangeEventArgs e)
    {
        var document = EditorContext.GetEditor().Document;

        // Start position (safe)
        var startLocation = document.GetLocation(e.Offset);

        TextLocation endLocation;

        if (e.RemovalLength > 0)
        {
            // Compute end position manually using removed text
            var removedText = e.RemovedText?.Text ?? string.Empty;

            var line = startLocation.Line;
            var column = startLocation.Column;

            foreach (var ch in removedText)
                if (ch == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }

            endLocation = new TextLocation(line, column);
        }
        else
        {
            // No removal → insertion only
            endLocation = startLocation;
        }

        return new DocumentChangeDto(
            new DocumentPosition(startLocation),
            new DocumentPosition(endLocation),
            e.InsertedText?.Text ?? string.Empty
        );
    }
}