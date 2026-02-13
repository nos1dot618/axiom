using System.IO;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Documents;

public sealed class DocumentManager(TextEditor textEditor)
{
    public bool SuppressChanges { get; private set; }

    public async Task<string> LoadFileAsync(string filePath)
    {
        SuppressChanges = true;
        var text = await File.ReadAllTextAsync(filePath);
        textEditor.Text = text;
        SuppressChanges = false;

        return text;
    }

    public async Task SaveFileAsync(string filePath)
    {
        await File.WriteAllTextAsync(filePath, textEditor.Text);
    }

    public DocumentChangeDto CreateChange(DocumentChangeEventArgs e)
    {
        var document = textEditor.Document;
        var startPosition = new DocumentPosition(document.GetLocation(e.Offset));
        var endPosition = new DocumentPosition(document.GetLocation(e.Offset + e.RemovalLength));

        return new DocumentChangeDto(startPosition, endPosition, e.InsertedText.Text ?? string.Empty);
    }
}