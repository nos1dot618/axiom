using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Axiom.Editor.Diagnostics;

public sealed class TextMarkerService :
    DocumentColorizingTransformer,
    IBackgroundRenderer,
    ITextMarkerService,
    ITextViewConnect
{
    private readonly TextSegmentCollection<TextMarker> _markers;
    private readonly TextDocument _document;
    private readonly List<TextView> _textViews = new();

    public TextMarkerService(TextDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _markers = new TextSegmentCollection<TextMarker>(_document);
    }

    #region ITextMarkerService

    public ITextMarker Create(int startOffset, int length)
    {
        int textLength = _document.TextLength;

        if (startOffset < 0 || startOffset > textLength)
            throw new ArgumentOutOfRangeException(nameof(startOffset));

        if (length < 0 || startOffset + length > textLength)
            throw new ArgumentOutOfRangeException(nameof(length));

        var marker = new TextMarker(this, startOffset, length);
        _markers.Add(marker);
        return marker;
    }

    public IEnumerable<ITextMarker> TextMarkers => _markers;

    public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset)
        => _markers.FindSegmentsContaining(offset);

    public void Remove(ITextMarker marker)
    {
        if (marker is TextMarker m && _markers.Remove(m))
        {
            Redraw(m);
            m.OnDeleted();
        }
    }

    public void RemoveAll(Predicate<ITextMarker> predicate)
    {
        foreach (var m in _markers.ToArray())
        {
            if (predicate(m))
                Remove(m);
        }
    }

    #endregion

    #region Rendering

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!_markers.Any() || !textView.VisualLinesValid)
            return;

        int viewStart = textView.VisualLines.First().FirstDocumentLine.Offset;
        int viewEnd = textView.VisualLines.Last().LastDocumentLine.EndOffset;

        foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                var geoBuilder = new BackgroundGeometryBuilder
                {
                    AlignToWholePixels = true,
                    CornerRadius = 3
                };

                geoBuilder.AddSegment(textView, marker);

                var geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    var brush = new SolidColorBrush(marker.BackgroundColor.Value);
                    brush.Freeze();
                    drawingContext.DrawGeometry(brush, null, geometry);
                }
            }

            var underlineTypes = TextMarkerTypes.SquigglyUnderline |
                                 TextMarkerTypes.NormalUnderline |
                                 TextMarkerTypes.DottedUnderline;

            if ((marker.MarkerTypes & underlineTypes) != 0)
            {
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var start = r.BottomLeft;
                    var end = r.BottomRight;

                    var brush = new SolidColorBrush(marker.MarkerColor);
                    brush.Freeze();

                    if (marker.MarkerTypes.HasFlag(TextMarkerTypes.SquigglyUnderline))
                    {
                        DrawSquiggly(drawingContext, start, end, brush);
                    }

                    if (marker.MarkerTypes.HasFlag(TextMarkerTypes.NormalUnderline))
                    {
                        var pen = new Pen(brush, 1);
                        pen.Freeze();
                        drawingContext.DrawLine(pen, start, end);
                    }

                    if (marker.MarkerTypes.HasFlag(TextMarkerTypes.DottedUnderline))
                    {
                        var pen = new Pen(brush, 1)
                        {
                            DashStyle = DashStyles.Dot
                        };
                        pen.Freeze();
                        drawingContext.DrawLine(pen, start, end);
                    }
                }
            }
        }
    }

    private static void DrawSquiggly(DrawingContext ctx, Point start, Point end, Brush brush)
    {
        double offset = 2.5;
        int count = Math.Max((int)((end.X - start.X) / offset) + 1, 4);

        var geometry = new StreamGeometry();
        using (var gc = geometry.Open())
        {
            gc.BeginFigure(start, false, false);

            for (int i = 0; i < count; i++)
            {
                gc.LineTo(
                    new Point(start.X + i * offset,
                        start.Y - ((i + 1) % 2 == 0 ? offset : 0)),
                    true,
                    false);
            }
        }

        geometry.Freeze();

        var pen = new Pen(brush, 1);
        pen.Freeze();

        ctx.DrawGeometry(Brushes.Transparent, pen, geometry);
    }

    #endregion

    #region Colorizing

    protected override void ColorizeLine(DocumentLine line)
    {
        foreach (var marker in _markers.FindOverlappingSegments(line.Offset, line.Length))
        {
            if (marker.ForegroundColor != null ||
                marker.FontWeight != null ||
                marker.FontStyle != null)
            {
                ChangeLinePart(
                    Math.Max(marker.StartOffset, line.Offset),
                    Math.Min(marker.EndOffset, line.EndOffset),
                    element =>
                    {
                        if (marker.ForegroundColor != null)
                        {
                            var brush = new SolidColorBrush(marker.ForegroundColor.Value);
                            brush.Freeze();
                            element.TextRunProperties.SetForegroundBrush(brush);
                        }

                        var tf = element.TextRunProperties.Typeface;
                        element.TextRunProperties.SetTypeface(
                            new Typeface(
                                tf.FontFamily,
                                marker.FontStyle ?? tf.Style,
                                marker.FontWeight ?? tf.Weight,
                                tf.Stretch));
                    });
            }
        }
    }

    #endregion

    #region TextView Connect

    void ITextViewConnect.AddToTextView(TextView textView)
    {
        if (!_textViews.Contains(textView))
            _textViews.Add(textView);
    }

    void ITextViewConnect.RemoveFromTextView(TextView textView)
    {
        _textViews.Remove(textView);
    }

    internal void Redraw(ISegment segment)
    {
        foreach (var view in _textViews)
            view.Redraw(segment, DispatcherPriority.Normal);
    }

    #endregion
}
