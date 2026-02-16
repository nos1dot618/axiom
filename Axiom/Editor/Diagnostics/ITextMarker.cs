using System.Windows;
using System.Windows.Media;

namespace Axiom.Editor.Diagnostics;

public interface ITextMarker
{
    int StartOffset { get; }
    int EndOffset { get; }
    int Length { get; }

    void Delete();
    bool IsDeleted { get; }

    event EventHandler? Deleted;

    Color? BackgroundColor { get; set; }
    Color? ForegroundColor { get; set; }

    FontWeight? FontWeight { get; set; }
    FontStyle? FontStyle { get; set; }

    TextMarkerTypes MarkerTypes { get; set; }
    Color MarkerColor { get; set; }

    object? Tag { get; set; }
    object? ToolTip { get; set; }
}