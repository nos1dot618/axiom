using System.Diagnostics;
using System.IO;
using System.Text;
using Axiom.Editor.Documents;
using Axiom.Infrastructure.Logging;

namespace Axiom.Editor.Build;

public class RunService : IRunService
{
    public async Task<int> BuildAsync(string command, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = ServicesRegistry.FileService.ProjectRoot
            },
            EnableRaisingEvents = true
        };

        ConfigureEnvironment(process.StartInfo);

        try
        {
            process.Start();

            var waitForExitTask = process.WaitForExitAsync(cancellationToken);
            var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
            var completedTask = await Task.WhenAny(waitForExitTask, cancellationTask);

            if (completedTask == cancellationTask)
            {
                try
                {
                    if (!process.HasExited) process.Kill(true);
                }
                catch (InvalidOperationException)
                {
                    // Already exited
                }
                catch (Exception ex)
                {
                    ErrorHandler.DisplayMessage($"Failed to kill process: {ex}");
                }

                throw new OperationCanceledException(cancellationToken);
            }

            await waitForExitTask;

            var projectSettings = ServicesRegistry.SettingsService.CurrentSettings.Project;
            if (projectSettings.BuildLogPath == null) return process.ExitCode;

            var logPath = ServicesRegistry.FileService.GetAbsolutePath(projectSettings.BuildLogPath);
            if (!File.Exists(logPath)) await File.Create(logPath).DisposeAsync();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(output) && string.IsNullOrWhiteSpace(error))
            {
                await File.WriteAllTextAsync(logPath, "Nothing to show.", cancellationToken);
            }
            else
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(output)) builder.AppendLine(output);
                if (!string.IsNullOrWhiteSpace(error)) builder.AppendLine(error);
                await File.WriteAllTextAsync(logPath, builder.ToString(), cancellationToken);
            }

            return process.ExitCode;
        }
        finally
        {
            process.Dispose();
        }
    }

    public void Run(string command)
    {
        var programStartInfo = new ProcessStartInfo
        {
            // TODO: Have a structure called RunCommand, which would store all the relevant information.
            //       And can be configured by the user through project config file.
            FileName = "cmd.exe",
            Arguments = $"/k {command}",
            UseShellExecute = false,
            CreateNoWindow = false,
            // TODO: Move working directory logic outside this function.
            //       User may want to change the working directory for some project.
            WorkingDirectory = ServicesRegistry.FileService.ProjectRoot
        };

        ConfigureEnvironment(programStartInfo);
        Process.Start(programStartInfo);
    }

    private static void ConfigureEnvironment(ProcessStartInfo startInfo)
    {
        var filepath = FileService.CurrentBuffer.Path;

        startInfo.Environment["FILEPATH"] = filepath;
        startInfo.Environment["FILEPATH_NO_EXT"] = Path.Combine(Path.GetDirectoryName(filepath)!,
            Path.GetFileNameWithoutExtension(filepath));
        startInfo.Environment["PROJECT_ROOT"] = ServicesRegistry.FileService.ProjectRoot;
    }
}