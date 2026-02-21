using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace Axiom.Editor.Diagnostics;

internal sealed class TextMarker : TextSegment, ITextMarker
{
    private readonly TextMarkerService _service;

    private Color? _backgroundColor;

    private FontStyle? _fontStyle;

    private FontWeight? _fontWeight;

    private Color? _foregroundColor;

    private Color _markerColor;

    private TextMarkerTypes _markerTypes;

    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
        MarkerTypes = TextMarkerTypes.None;
    }

    public event EventHandler? Deleted;

    public bool IsDeleted => !IsConnectedToCollection;

    public void Delete()
    {
        _service.Remove(this);
    }

    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor == value) return;
            _backgroundColor = value;
            Redraw();
        }
    }

    public Color? ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (_foregroundColor == value) return;
            _foregroundColor = value;
            Redraw();
        }
    }

    public FontWeight? FontWeight
    {
        get => _fontWeight;
        set
        {
            if (_fontWeight == value) return;
            _fontWeight = value;
            Redraw();
        }
    }

    public FontStyle? FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle == value) return;
            _fontStyle = value;
            Redraw();
        }
    }

    public TextMarkerTypes MarkerTypes
    {
        get => _markerTypes;
        set
        {
            if (_markerTypes == value) return;
            _markerTypes = value;
            Redraw();
        }
    }

    public Color MarkerColor
    {
        get => _markerColor;
        set
        {
            if (_markerColor == value) return;
            _markerColor = value;
            Redraw();
        }
    }

    public object? Tag { get; set; }

    public object? ToolTip { get; set; }

    internal void OnDeleted()
    {
        Deleted?.Invoke(this, EventArgs.Empty);
    }

    private void Redraw()
    {
        _service.Redraw(this);
    }
}