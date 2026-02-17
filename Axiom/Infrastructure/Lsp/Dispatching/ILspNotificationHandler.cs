using System.Text.Json;

namespace Axiom.Infrastructure.Lsp.Dispatching;

public interface ILspNotificationHandler
{
    string Method { get; }
    Task HandleAsync(JsonElement payload);
}