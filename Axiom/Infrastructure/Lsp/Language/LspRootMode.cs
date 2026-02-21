using System.IO;
using Axiom.Core.Services;
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

            LspRootMode.FileDirectory => DocumentManager.CurrentDocumentFilepath == null
                ? throw new InvalidOperationException(
                    "Failed to determine directory, because current document URI is null.")
                : Path.GetDirectoryName(DocumentManager.CurrentDocumentFilepath) ??
                  throw new InvalidOperationException("Failed to determine directory from current document URI."),

            LspRootMode.Fixed =>
                string.IsNullOrWhiteSpace(ServiceFactory.SettingsService.CurrentSettings.Lsp.FixedRootPath)
                    ? throw new InvalidOperationException("Fixed root path is not set.")
                    : ServiceFactory.SettingsService.CurrentSettings.Lsp.FixedRootPath,

            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}