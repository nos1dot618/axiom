using System.IO;
using Axiom.Editor;
using Axiom.Editor.Documents;

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
    Fixed
}

public static class LspRootModeExtensions
{
    public static string GetRootPath(this LspRootMode mode)
    {
        return mode switch
        {
            LspRootMode.Workspace => Directory.GetCurrentDirectory(),

            LspRootMode.FileDirectory => FileService.CurrentBuffer.IsVirtual
                ? throw new InvalidOperationException(
                    "Failed to determine directory, because current document URI is null.")
                : Path.GetDirectoryName(FileService.CurrentBuffer.Path) ??
                  throw new InvalidOperationException("Failed to determine directory from current document URI."),

            LspRootMode.Fixed =>
                string.IsNullOrWhiteSpace(ServicesRegistry.SettingsService.CurrentSettings.Lsp.FixedRootPath)
                    ? throw new InvalidOperationException("Fixed root path is not set.")
                    : ServicesRegistry.SettingsService.CurrentSettings.Lsp.FixedRootPath,

            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}