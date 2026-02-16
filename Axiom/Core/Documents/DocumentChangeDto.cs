namespace Axiom.Core.Documents;

public sealed record DocumentChangeDto(DocumentPosition Start, DocumentPosition End, string Text);