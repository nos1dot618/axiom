using System.Windows.Input;
using Axiom.Infrastructure.Logging;

namespace Axiom.UI.Commands;

public static class AsyncCommand
{
    public static ExecutedRoutedEventHandler Create(Func<Task> execute)
    {
        return async void (_, _) =>
        {
            try
            {
                await execute();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        };
    }
}