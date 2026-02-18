using Axiom.Infrastructure.Logging;

namespace Axiom.UI.Commands;

public static class AsyncCommand
{
    public static async void Execute(Func<Task> execute)
    {
        try
        {
            await execute();
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
        }
    }
}