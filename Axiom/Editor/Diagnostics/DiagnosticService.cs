using System.Text;
using System.Windows.Media;
using Axiom.Core.Diagnostics;
using ICSharpCode.AvalonEdit;

namespace Axiom.Editor.Diagnostics;

public sealed class DiagnosticService
{
    private readonly TextEditor _editor;
    private readonly ITextMarkerService _markerService;

    public DiagnosticService(TextEditor editor)
    {
        _editor = editor;

        var textMarkerService = new TextMarkerService(editor.Document);
        _markerService = textMarkerService;

        editor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
        editor.TextArea.TextView.LineTransformers.Add(textMarkerService);
    }

    public void Update(IEnumerable<Diagnostic> diagnostics)
    {
        // Remove old diagnostics
        _markerService.RemoveAll(_ => true);

        foreach (var diagnostic in diagnostics)
        {
            var (offset, length) = ConvertToOffsets(diagnostic);
            if (length <= 0)
                continue;

            var marker = _markerService.Create(offset, length);

            ApplyStyle(marker, diagnostic);

            marker.ToolTip = BuildTooltip(diagnostic);
            marker.Tag = diagnostic;
        }
    }

    public void Clear()
    {
        _markerService.RemoveAll(_ => true);
    }

    private (int offset, int length) ConvertToOffsets(Diagnostic d)
    {
        int start = _editor.Document.GetOffset(
            d.StartPosition.Row + 1,
            d.StartPosition.Column + 1);

        int end = _editor.Document.GetOffset(
            d.EndPosition.Row + 1,
            d.EndPosition.Column + 1);

        return (start, end - start);
    }

    private static void ApplyStyle(ITextMarker marker, Diagnostic d)
    {
        switch (d.Severity)
        {
            case DiagnosticSeverity.Error:
                marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                marker.MarkerColor = Colors.Red;
                break;

            case DiagnosticSeverity.Warning:
                marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                marker.MarkerColor = Colors.Orange;
                break;

            case DiagnosticSeverity.Info:
                marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                marker.MarkerColor = Colors.DodgerBlue;
                break;

            case DiagnosticSeverity.Hint:
                marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                marker.MarkerColor = Colors.Gray;
                break;
        }
    }

    private static string BuildTooltip(Diagnostic d)
    {
        var sb = new StringBuilder();

        sb.AppendLine(d.Message);

        if (!string.IsNullOrWhiteSpace(d.Code))
            sb.AppendLine($"Code: {d.Code}");

        if (!string.IsNullOrWhiteSpace(d.Source))
            sb.AppendLine($"Source: {d.Source}");

        if (!string.IsNullOrWhiteSpace(d.CodeDescriptionUrl))
            sb.AppendLine($"Docs: {d.CodeDescriptionUrl}");

        return sb.ToString();
    }
}