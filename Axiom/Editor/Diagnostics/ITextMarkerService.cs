namespace Axiom.Editor.Diagnostics;

public interface ITextMarkerService
{
    ITextMarker Create(int startOffset, int length);

    IEnumerable<ITextMarker> TextMarkers { get; }

    void Remove(ITextMarker marker);

    void RemoveAll(Predicate<ITextMarker> predicate);

    IEnumerable<ITextMarker> GetMarkersAtOffset(int offset);
}