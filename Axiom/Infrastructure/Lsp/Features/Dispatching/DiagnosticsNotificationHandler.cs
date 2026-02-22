using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Axiom.Core.Diagnostics;
using Axiom.Editor;
using Axiom.Infrastructure.Lsp.Features.Mapping;
using Axiom.Infrastructure.Lsp.Protocol;

namespace Axiom.Infrastructure.Lsp.Features.Dispatching;

public sealed class DiagnosticsNotificationHandler : ILspNotificationHandler
{
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private readonly IFeatureMapper<Diagnostic> _mapper = new DiagnosticMapper();

    public string Method => LspMethod.Notification.Diagnostics;

    public Task HandleAsync(JsonElement payload)
    {
        var diagnostics = _mapper.Map(payload);
        ServiceFactory.LspSession.DiagnosticService?.Update(diagnostics);
        return Task.CompletedTask;
    }
}