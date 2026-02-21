using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Axiom.Core.Diagnostics;
using Axiom.Core.Services;
using Axiom.Core.Settings;
using ICSharpCode.AvalonEdit;

namespace Axiom.Editor.Diagnostics;

public sealed class DiagnosticService : IDisposable
{
    private readonly TextEditor _editor;
    private readonly ITextMarkerService _markerService;
    private readonly EditorSettings _settings;
    private readonly TextMarkerService _textMarkerService;
    private bool _isDisposed;

    public DiagnosticService(TextEditor editor)
    {
        _editor = editor;

        _textMarkerService = new TextMarkerService(editor.Document);
        _markerService = _textMarkerService;
        _settings = ServiceFactory.SettingsService.CurrentSettings;

        editor.TextArea.TextView.BackgroundRenderers.Add(_textMarkerService);
        editor.TextArea.TextView.LineTransformers.Add(_textMarkerService);

        editor.TextArea.TextView.MouseHover += OnTextViewMouseHover;
        editor.TextArea.TextView.MouseHoverStopped += OnTextViewMouseHoverStopped;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Clear markers.
        _markerService.RemoveAll(_ => true);

        // Remove renderers.
        _editor.TextArea.TextView.BackgroundRenderers.Remove(_textMarkerService);
        _editor.TextArea.TextView.LineTransformers.Remove(_textMarkerService);

        // Unsubscribe events.
        _editor.TextArea.TextView.MouseHover -= OnTextViewMouseHover;
        _editor.TextArea.TextView.MouseHoverStopped -= OnTextViewMouseHoverStopped;

        // Close tooltip.
        if (_editor.ToolTip is ToolTip tooltip) tooltip.IsOpen = false;
        _editor.ToolTip = null;
    }

    public void Update(IEnumerable<Diagnostic> diagnostics)
    {
        if (!_editor.Dispatcher.CheckAccess())
        {
            _editor.Dispatcher.BeginInvoke(() => Update(diagnostics));
            return;
        }

        _markerService.RemoveAll(_ => true);

        foreach (var diagnostic in diagnostics)
        {
            var (offset, length) = ConvertToOffsets(diagnostic);
            if (length <= 0) continue;

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
        var start = _editor.Document.GetOffset(d.StartPosition.Row + 1, d.StartPosition.Column + 1);
        var end = _editor.Document.GetOffset(d.EndPosition.Row + 1, d.EndPosition.Column + 1);
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

        marker.ToolTip = d.Message;
    }

    private static string BuildTooltip(Diagnostic d)
    {
        var sb = new StringBuilder();
        sb.AppendLine(d.Message);
        if (!string.IsNullOrWhiteSpace(d.Code)) sb.AppendLine($"Code: {d.Code}");
        if (!string.IsNullOrWhiteSpace(d.Source)) sb.AppendLine($"Source: {d.Source}");
        if (!string.IsNullOrWhiteSpace(d.CodeDescriptionUrl)) sb.AppendLine($"Docs: {d.CodeDescriptionUrl}");
        return sb.ToString();
    }

    private void OnTextViewMouseHover(object sender, MouseEventArgs e)
    {
        var position = _editor.TextArea.TextView.GetPositionFloor(e.GetPosition(_editor.TextArea.TextView));
        if (position == null) return;

        var offset = _editor.Document.GetOffset(position.Value.Location);
        var marker = _markerService.GetMarkersAtOffset(offset).FirstOrDefault();

        if (marker != null && !string.IsNullOrWhiteSpace((string)marker.ToolTip!))
            _editor.ToolTip = new ToolTip
            {
                Style = (Style)Application.Current.Resources["AxiomDiagnosticToolTipStyle"]!,
                Tag = new SolidColorBrush(marker.MarkerColor),
                Content = new TextBlock
                {
                    Text = marker.ToolTip.ToString(),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 500,
                    FontFamily = new FontFamily(_settings.Editor.FontFamily)
                },
                PlacementTarget = _editor.TextArea,
                Placement = PlacementMode.Mouse,
                StaysOpen = false,
                IsOpen = true
            };
    }

    private void OnTextViewMouseHoverStopped(object sender, MouseEventArgs e)
    {
        _editor.ToolTip = null;
    }
}