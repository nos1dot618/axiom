using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axiom.App;
using Axiom.Common;
using Axiom.Editor;
using Axiom.Infrastructure.Logging;

namespace Axiom.UI.Project;

public sealed class ProjectUiController
{
    private static TextBlock? _timerText;
    private static TextBlock? _statusText;
    private static CancellationTokenSource? _buildCts;
    private static bool _isBuilding;
    private static ReadOnlyWindow? _buildLogWindow;
    private static Window? _mainWindow;
    private static readonly Stopwatch Stopwatch = new();

    private static readonly DispatcherTimer Timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(100)
    };

    public ProjectUiController(TextBlock timerText, TextBlock statusText, Window mainWindow)
    {
        _timerText = timerText;
        _statusText = statusText;
        _mainWindow = mainWindow;
        Timer.Tick += (_, _) => { timerText.Text = Stopwatch.Elapsed.ToString(@"mm\:ss"); };
    }

    public static ICommand BuildProjectCommand { get; } = new AsyncRelayCommand(_ => BuildAsync());
    public static ICommand RunProjectCommand { get; } = new AsyncRelayCommand(_ => RunAsync());
    public static ICommand StopBuildCommand { get; } = new RelayCommand(_ => StopBuild());
    public static ICommand ShowBuildLogsCommand { get; } = new AsyncRelayCommand(_ => ShowBuildLogs());

    private static async Task<int> BuildAsync()
    {
        if (_isBuilding) return -1;

        _isBuilding = true;
        _buildCts = new CancellationTokenSource();

        if (_statusText != null) _statusText.Text = "Building...";
        if (_timerText != null) _timerText.Text = "00:00";

        Stopwatch.Restart();
        Timer.Start();

        int code;

        try
        {
            code = await ServicesRegistry.RunService.BuildAsync(
                ServicesRegistry.SettingsService.CurrentSettings.Project.BuildFormat, _buildCts.Token);
        }
        catch (OperationCanceledException)
        {
            code = -1;
        }

        Timer.Stop();
        Stopwatch.Stop();

        var cts = _buildCts; // snapshot
        var wasCancelled = cts.IsCancellationRequested;
        var statusMessage = wasCancelled ? "Build Cancelled" :
            code == 0 ? "Built Successfully" : $"Build Failed (Exit {code})";

        _statusText!.Text = statusMessage;
        _isBuilding = false;
        cts.Dispose();
        _buildCts = null;

        return code;
    }

    private static async Task RunAsync()
    {
        var projectSettings = ServicesRegistry.SettingsService.CurrentSettings.Project;
        if (projectSettings.BuildBeforeRun)
        {
            var code = await BuildAsync();
            switch (code)
            {
                case -1:
                    ErrorHandler.DisplayMessage("Build already going on.");
                    return;
                case 0:
                    break;
                default:
                    ErrorHandler.DisplayMessage($"Build Failed with exit code {code}.");
                    return;
            }
        }

        ServicesRegistry.RunService.Run(projectSettings.RunFormat);
    }

    private static void StopBuild()
    {
        if (!_isBuilding) return;
        _buildCts?.Cancel();
    }

    private static async Task ShowBuildLogs()
    {
        // TODO: Support polling for changes in the build log.
        // TODO: Add notification to the editor window for build log update.
        // TODO: Move focus to the log window if already exists.
        if (_buildLogWindow is not { IsVisible: true })
        {
            var projectSettings = ServicesRegistry.SettingsService.CurrentSettings.Project;
            if (projectSettings.BuildLogPath == null)
            {
                ErrorHandler.DisplayMessage("Build Log Path is not set. Please set it inside configuration file.");
                return;
            }

            var logPath = ServicesRegistry.FileService.GetAbsolutePath(projectSettings.BuildLogPath);
            if (!File.Exists(logPath))
            {
                ErrorHandler.DisplayMessage("Build Log File does not exist. Please build the project.");
                return;
            }

            _buildLogWindow = new ReadOnlyWindow("Build Log", await File.ReadAllTextAsync(logPath))
            {
                Owner = _mainWindow
            };
            _buildLogWindow.Show();
        }
        else
        {
            ErrorHandler.DisplayMessage("Log file is already opened in a separate window.");
        }
    }
}