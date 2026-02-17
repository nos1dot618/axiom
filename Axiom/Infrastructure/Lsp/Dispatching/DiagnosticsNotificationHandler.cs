using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Axiom.Core;
using Axiom.Core.Diagnostics;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Infrastructure.Lsp.Dispatching;

public sealed class DiagnosticsNotificationHandler : ILspNotificationHandler
{
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private readonly IServiceMapper<Diagnostic> _mapper = new DiagnosticMapper();

    public string Method => LspMethod.Notification.Diagnostics;

    public Task HandleAsync(JsonElement payload)
    {
        var diagnostics = _mapper.Map(payload);
        DocumentContextProvider.Get()?.DiagnosticService.Update(diagnostics);
        return Task.CompletedTask;
    }
}