using System.IO;

namespace Axiom.Infrastructure.Lsp.Language;

public static class LanguageIdResolver
{
    private static readonly Dictionary<string, string> ExtensionMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".cs"] = "csharp",
            [".py"] = "python",
            [".js"] = "javascript",
            [".ts"] = "typescript",
            [".tsx"] = "typescriptreact",
            [".jsx"] = "javascriptreact",
            [".json"] = "json",
            [".html"] = "html",
            [".css"] = "css",
            [".xml"] = "xml",
            [".xaml"] = "xaml",
            [".md"] = "markdown",
            [".cpp"] = "cpp",
            [".c"] = "c",
            [".h"] = "cpp",
            [".java"] = "java",
            [".go"] = "go",
            [".rs"] = "rust",
            [".php"] = "php",
            [".rb"] = "ruby",
            [".yaml"] = "yaml",
            [".yml"] = "yaml",
            [".sh"] = "shellscript"
        };

    public static string? GetLanguageId(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;
        var extension = Path.GetExtension(filePath);
        return ExtensionMap.GetValueOrDefault(extension);
    }
}