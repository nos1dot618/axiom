namespace Axiom.Completion;

public sealed class CompletionContextDto(string? triggerCharacter)
{
    private int TriggerKind { get; } = triggerCharacter != null ? 2 : 1;
    private string? TriggerCharacter { get; } = triggerCharacter;

    public object ToDto()
    {
        if (TriggerCharacter == null)
            return new
            {
                triggerKind = TriggerKind
            };

        return new
        {
            triggerKind = TriggerKind,
            triggerCharacter = TriggerCharacter
        };
    }
}