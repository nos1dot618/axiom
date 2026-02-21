using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Axiom.Editor.Diagnostics;

public sealed class TextMarkerService : DocumentColorizingTransformer, IBackgroundRenderer, ITextMarkerService,
    ITextViewConnect
{
    private readonly TextDocument _document;
    private readonly TextSegmentCollection<TextMarker> _markers;
    private readonly List<TextView> _textViews = [];

    public TextMarkerService(TextDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _markers = new TextSegmentCollection<TextMarker>(_document);
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_markers.Count == 0 || !textView.VisualLinesValid) return;

        var viewStart = textView.VisualLines.First().FirstDocumentLine.Offset;
        var viewEnd = textView.VisualLines.Last().LastDocumentLine.EndOffset;

        foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                geoBuilder.AddSegment(textView, marker);

                var geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    var brush = new SolidColorBrush(marker.BackgroundColor.Value);
                    brush.Freeze();
                    drawingContext.DrawGeometry(brush, null, geometry);
                }
            }

            if ((marker.MarkerTypes & TextMarkerTypes.UnderlineTypes) == 0) continue;

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                var start = rect.BottomLeft;
                var end = rect.BottomRight;

                var brush = new SolidColorBrush(marker.MarkerColor);
                brush.Freeze();

                if (marker.MarkerTypes.HasFlag(TextMarkerTypes.SquigglyUnderline))
                {
                    const double offset = 2.5;
                    var count = Math.Max((int)((end.X - start.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();
                    using (var geometryContext = geometry.Open())
                    {
                        geometryContext.BeginFigure(start, false, false);
                        geometryContext.PolyLineTo(CreatePoints(start, end, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    drawingContext.DrawGeometry(Brushes.Transparent, pen, geometry);
                }

                if (marker.MarkerTypes.HasFlag(TextMarkerTypes.NormalUnderline))
                {
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    drawingContext.DrawLine(pen, start, end);
                }

                // ReSharper disable once InvertIf
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

    public IEnumerable<ITextMarker> TextMarkers => _markers;

    public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset)
    {
        return _markers.FindSegmentsContaining(offset);
    }

    public ITextMarker Create(int startOffset, int length)
    {
        var textLength = _document.TextLength;

        if (startOffset < 0 || startOffset > textLength) throw new ArgumentOutOfRangeException(nameof(startOffset));
        if (length < 0 || startOffset + length > textLength) throw new ArgumentOutOfRangeException(nameof(length));

        var marker = new TextMarker(this, startOffset, length);
        _markers.Add(marker);
        return marker;
    }

    public void Remove(ITextMarker marker)
    {
        if (marker is not TextMarker m || !_markers.Remove(m)) return;

        Redraw(m);
        m.OnDeleted();
    }

    public void RemoveAll(Predicate<ITextMarker> predicate)
    {
        foreach (var m in _markers.ToArray())
            if (predicate(m))
                Remove(m);
    }


    void ITextViewConnect.AddToTextView(TextView textView)
    {
        if (!_textViews.Contains(textView)) _textViews.Add(textView);
    }

    void ITextViewConnect.RemoveFromTextView(TextView textView)
    {
        _textViews.Remove(textView);
    }

    private static IEnumerable<Point> CreatePoints(Point start, Point _, double offset, int count)
    {
        for (var i = 0; i < count; i++)
            yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        foreach (var marker in _markers.FindOverlappingSegments(line.Offset, line.Length))
            if (marker.ForegroundColor != null || marker.FontWeight != null || marker.FontStyle != null)
                ChangeLinePart(Math.Max(marker.StartOffset, line.Offset), Math.Min(marker.EndOffset, line.EndOffset),
                    element =>
                    {
                        if (marker.ForegroundColor != null)
                        {
                            var brush = new SolidColorBrush(marker.ForegroundColor.Value);
                            brush.Freeze();
                            element.TextRunProperties.SetForegroundBrush(brush);
                        }

                        var typeface = element.TextRunProperties.Typeface;
                        element.TextRunProperties.SetTypeface(
                            new Typeface(
                                typeface.FontFamily,
                                marker.FontStyle ?? typeface.Style,
                                marker.FontWeight ?? typeface.Weight,
                                typeface.Stretch));
                    }
                );
    }

    internal void Redraw(ISegment segment)
    {
        foreach (var view in _textViews) view.Redraw(segment);
    }
}