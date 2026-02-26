using System.Windows.Input;
using Axiom.Infrastructure.Logging;

namespace Axiom.Common;

public class AsyncRelayCommand(Func<object?, Task> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public async void Execute(object? parameter)
    {
        try
        {
            await execute(parameter);
        }
        catch (Exception e)
        {
            ErrorHandler.HandleException(e);
        }
    }
}