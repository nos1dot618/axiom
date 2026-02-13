namespace Axiom.Documents;

public sealed record DocumentChangeDto(DocumentPosition Start, DocumentPosition End, string Text);