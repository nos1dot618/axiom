using System.IO;
using System.Runtime.InteropServices;

namespace Axiom.Common;

public static class CommandResolver
{
    public static string? ResolveFromPath(string command)
    {
        // If already absolute pat: return as is.
        if (Path.IsPathRooted(command) && File.Exists(command)) return command;

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv)) return null;
        var paths = pathEnv.Split(Path.PathSeparator);

        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [""];

        return (from path in paths from ext in extensions select Path.Combine(path, command + ext))
            .FirstOrDefault(File.Exists);
    }
}