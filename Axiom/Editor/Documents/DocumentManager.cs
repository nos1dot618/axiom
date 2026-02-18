using System.IO;
using Axiom.Core.Documents;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor.Documents;

public sealed class DocumentManager(TextEditor textEditor)
{
    public bool SuppressChanges { get; private set; }

    public async Task<string> LoadFileAsync(string filePath)
    {
        SuppressChanges = true;
        var text = await File.ReadAllTextAsync(filePath);
        textEditor.Text = text;
        SuppressChanges = false;

        DocumentContextProvider.SetCurrentDocument(new Uri(filePath).AbsoluteUri);
        DocumentContextProvider.Create();

        return text;
    }

    public static void CloseFile()
    {
        DocumentContextProvider.Close();
    }

    public async Task SaveFileAsync(string filePath)
    {
        await File.WriteAllTextAsync(filePath, textEditor.Text);
    }

    public DocumentChangeDto CreateChange(DocumentChangeEventArgs e)
    {
        var document = textEditor.Document;

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
            {
                if (ch == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
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