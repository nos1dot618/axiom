using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor.Diagnostics;

internal sealed class TextMarker : TextSegment, ITextMarker
{
    private readonly TextMarkerService _service;

    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
        MarkerTypes = TextMarkerTypes.None;
    }

    public event EventHandler? Deleted;

    public bool IsDeleted => !IsConnectedToCollection;

    public void Delete() => _service.Remove(this);

    internal void OnDeleted() => Deleted?.Invoke(this, EventArgs.Empty);

    private void Redraw() => _service.Redraw(this);

    private Color? _backgroundColor;

    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                Redraw();
            }
        }
    }

    private Color? _foregroundColor;

    public Color? ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (_foregroundColor != value)
            {
                _foregroundColor = value;
                Redraw();
            }
        }
    }

    private FontWeight? _fontWeight;

    public FontWeight? FontWeight
    {
        get => _fontWeight;
        set
        {
            if (_fontWeight != value)
            {
                _fontWeight = value;
                Redraw();
            }
        }
    }

    private FontStyle? _fontStyle;

    public FontStyle? FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                Redraw();
            }
        }
    }

    private TextMarkerTypes _markerTypes;

    public TextMarkerTypes MarkerTypes
    {
        get => _markerTypes;
        set
        {
            if (_markerTypes != value)
            {
                _markerTypes = value;
                Redraw();
            }
        }
    }

    private Color _markerColor;

    public Color MarkerColor
    {
        get => _markerColor;
        set
        {
            if (_markerColor != value)
            {
                _markerColor = value;
                Redraw();
            }
        }
    }

    public object? Tag { get; set; }
    public object? ToolTip { get; set; }
}