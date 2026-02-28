using System.Windows.Forms;

namespace Axiom.Infrastructure.Logging;

public static class ErrorHandler
{
    public static void HandleException(Exception ex)
    {
        // TODO: Use logger to report error.
        Console.Write(ex.StackTrace);
        MessageBox.Show($"{ex.Message}");
    }

    public static void DisplayMessage(string message)
    {
        MessageBox.Show($"{message}");
    }
}