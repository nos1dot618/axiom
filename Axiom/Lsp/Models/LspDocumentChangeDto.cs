namespace Axiom.Lsp.Models;

public sealed record LspDocumentChangeDto(LspDocumentPosition Start, LspDocumentPosition End, string Text);