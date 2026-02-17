 using System.Text.Json;
using Axiom.Core.Diagnostics;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Infrastructure.Lsp.Dispatching;

public sealed class DiagnosticsNotificationHandler : ILspNotificationHandler
{
    public string Method => LspMethod.Notification.Diagnostics;

    public Task HandleAsync(JsonElement payload)
    {
        var diagnostics = DiagnosticMapper.Map(payload);
        DocumentContextProvider.Get()?.DiagnosticService.Update(diagnostics);
        return Task.CompletedTask;
    }
}