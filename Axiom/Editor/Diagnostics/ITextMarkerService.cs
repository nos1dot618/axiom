namespace Axiom.Editor.Diagnostics;

public interface ITextMarkerService
{
    IEnumerable<ITextMarker> TextMarkers { get; }
    ITextMarker Create(int startOffset, int length);

    void Remove(ITextMarker marker);

    void RemoveAll(Predicate<ITextMarker> predicate);

    IEnumerable<ITextMarker> GetMarkersAtOffset(int offset);
}