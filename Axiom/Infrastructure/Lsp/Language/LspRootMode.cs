using System.IO;
using Axiom.Editor;
using Axiom.Editor.Documents;
using Axiom.Editor.Settings;
using Axiom.Infrastructure.Logging;

namespace Axiom.Infrastructure.Lsp.Language;

public enum LspRootMode
{
    /// <summary>
    ///     Assume root path as current working directory.
    /// </summary>
    Workspace,

    /// <summary>
    ///     Assume root path as the parent of the open file.
    /// </summary>
    FileDirectory,

    /// <summary>
    ///     Assume root path as the value of FixedRootPath.
    /// </summary>
    Fixed,

    /// <summary>
    ///     Assume root path as the parent of project config folder.
    /// </summary>
    Project
}

public static class LspRootModeExtensions
{
    public static string GetRootPath(this LspRootMode mode)
    {
        try
        {
            return mode switch
            {
                LspRootMode.Workspace => Directory.GetCurrentDirectory(),
                LspRootMode.FileDirectory => GetFileDirectory(),
                LspRootMode.Fixed => GetFixedRootPath(),
                LspRootMode.Project => GetProjectRootPath(),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex);
            // Fallback to workspace
            return Directory.GetCurrentDirectory();
        }
    }

    private static string GetFileDirectory()
    {
        if (FileService.CurrentBuffer.IsVirtual)
            throw new InvalidOperationException(
                "Failed to determine root directory, because current buffer is temporary.");

        return Path.GetDirectoryName(FileService.CurrentBuffer.Path) ??
               throw new InvalidOperationException("Failed to determine root directory from current document URI.");
    }

    private static string GetFixedRootPath()
    {
        var fixedPath = ServicesRegistry.SettingsService.CurrentSettings.Lsp.FixedRootPath;

        return string.IsNullOrWhiteSpace(fixedPath)
            ? throw new InvalidOperationException(
                "Fixed root path is not set. Please set it inside configuration file.")
            : fixedPath;
    }

    private static string GetProjectRootPath()
    {
        return SettingsService.ProjectPath?.FullName ??
               throw new InvalidOperationException("Project root path is not set properly.");
    }
}