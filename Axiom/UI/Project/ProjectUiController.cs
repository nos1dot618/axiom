using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axiom.Common;
using Axiom.Editor;
using Axiom.Infrastructure.Logging;

namespace Axiom.UI.Project;

public sealed class ProjectUiController
{
    private static TextBlock? _timerText;
    private static TextBlock? _statusText;
    private static readonly Stopwatch Stopwatch = new();

    private static readonly DispatcherTimer Timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(100)
    };

    public ProjectUiController(TextBlock timerText, TextBlock statusText)
    {
        _timerText = timerText;
        _statusText = statusText;
        Timer.Tick += (_, _) => { timerText.Text = Stopwatch.Elapsed.ToString(@"mm\:ss"); };
    }

    public static ICommand BuildProjectCommand { get; } = new AsyncRelayCommand(_ => Build());
    public static ICommand RunProjectCommand { get; } = new AsyncRelayCommand(_ => Run());

    private static async Task<int> Build()
    {
        if (_statusText != null) _statusText.Text = "Building...";
        if (_timerText != null) _timerText.Text = "00:00";

        Stopwatch.Restart();
        Timer.Start();

        var code = await Task.Run(() =>
            ServicesRegistry.RunService.Build(ServicesRegistry.SettingsService.CurrentSettings.Project.BuildFormat));

        Timer.Stop();
        Stopwatch.Stop();

        if (_statusText != null)
            _statusText.Text = code == 0 ? "Built Successfully" : $"Build Failed with exit code {code}.";

        return code;
    }

    private static async Task Run()
    {
        var projectSettings = ServicesRegistry.SettingsService.CurrentSettings.Project;
        if (projectSettings.BuildBeforeRun)
        {
            var code = await Build();
            if (code != 0)
            {
                ErrorHandler.DisplayMessage($"Build Failed with exit code {code}.");
                return;
            }
        }

        ServicesRegistry.RunService.Run(projectSettings.RunFormat);
    }
}