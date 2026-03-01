namespace Axiom.Editor.Build;

public interface IRunService
{
    public int Build(string command);
    public void Run(string command);
}