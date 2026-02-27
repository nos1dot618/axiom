namespace Axiom.Core.Documents;

public class DocumentAddress
{
    private static int _untitledCount = 1;

    public DocumentAddress()
    {
        Path = $"untitled:Untitled-{_untitledCount++}";
        IsVirtual = true;
    }

    public DocumentAddress(string path)
    {
        Path = new Uri(path).AbsolutePath;
        IsVirtual = false;
    }

    public string Path { get; }
    public bool IsVirtual { get; }
}