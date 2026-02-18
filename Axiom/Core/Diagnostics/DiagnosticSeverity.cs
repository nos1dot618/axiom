namespace Axiom.Core.Diagnostics;

public enum DiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Info = 3,
    Hint = 4
}

public static class DiagnosticSeverityExtensions
{
    public static DiagnosticSeverity? ToDiagnosticSeverity(this int value) =>
        Enum.IsDefined(typeof(DiagnosticSeverity), value) ? (DiagnosticSeverity)value : null;
}