using System.Text.Json;

namespace Axiom.Infrastructure.Lsp.Features.Mapping;

public interface IFeatureMapper<out T>
{
    public string ResultSetName { get; }

    public IReadOnlyList<T> Map(JsonElement result)
    {
        var itemsElement = ExtractItems(result);
        var resultSet = new List<T>();

        if (itemsElement.ValueKind != JsonValueKind.Array) return resultSet;

        resultSet.AddRange(itemsElement.EnumerateArray().Select(MapSingle));
        return resultSet;
    }

    public JsonElement ExtractItems(JsonElement result)
    {
        return result.ValueKind switch
        {
            JsonValueKind.Array => result,
            JsonValueKind.Object when result.TryGetProperty(ResultSetName, out var items) => items,
            _ => default
        };
    }

    public T MapSingle(JsonElement item);
}