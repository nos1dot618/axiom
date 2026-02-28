using System.Diagnostics;

namespace Axiom.Editor.Build;

public class RunService : IRunService
{
    public void Run(string command)
    {
        var programStartInfo = new ProcessStartInfo
        {
            // TODO: Have a structure called RunCommand, which would store all the relevant information.
            //       And can be configured by the user through project config file.
            FileName = "cmd.exe",
            Arguments = $"/k {command}",
            UseShellExecute = true,
            CreateNoWindow = false,
            // TODO: Move working directory logic outside this function.
            //       User may want to change the working directory for some project.
            WorkingDirectory = ServicesRegistry.FileService.ProjectRoot
        };

        Process.Start(programStartInfo);
    }
}