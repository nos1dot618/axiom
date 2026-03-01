namespace Axiom.Editor.Build;

public interface IRunService
{
    public Task<int> BuildAsync(string command, CancellationToken cancellationToken);
    public void Run(string command);
}