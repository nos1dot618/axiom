using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Axiom.Editor.Documents;

namespace Axiom.Editor.Build;

public class RunService : IRunService
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Build(string command)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}", // /c executes and exits.
            UseShellExecute = false,
            CreateNoWindow = true, // No visible window.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = ServicesRegistry.FileService.ProjectRoot
        };

        ConfigureEnvironment(process.StartInfo);
        process.Start();
        process.WaitForExit();

        var projectSettings = ServicesRegistry.SettingsService.CurrentSettings.Project;
        if (projectSettings.BuildLogPath == null) return process.ExitCode;

        var logPath = ServicesRegistry.FileService.GetAbsolutePath(projectSettings.BuildLogPath);
        if (!File.Exists(logPath))
            // Create the file and immediately close it
            using (File.Create(logPath))
            {
                ;
            }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (!string.IsNullOrWhiteSpace(output)) File.AppendAllText(logPath, output);
        if (!string.IsNullOrWhiteSpace(error)) File.AppendAllText(logPath, error);

        return process.ExitCode;
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