> _Native windows text editor in C# with multi language and LSP support._

![Thumbnail](./Resources/Pictures/Thumbnail.png)

> [!Important]
> Work in Progress

## Getting Started

```powershell
dotnet restore
dotnet build axiom.sln
```

> Requires .NET 8 SDK (Windows Desktop Runtime).

Run the Application:

```powershell
dotnet run --project Axiom/Axiom.csproj
```

Build Release version:

```powershell
dotnet build -c Release
```

> The executable will be located inside `Axiom\bin\Release\net8.0-windows\`.

Publish Standalone Executable (create a self-contained Windows executable):

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

> The executable will be located inside `Axiom\bin\Release\net8.0-windows\win-x64\publish\`. <br>
> This produces a runnable .exe without requiring .NET installation.

## Notes

1. This is a **Windows-only** application due to WPF.
2. Make sure you are using the **.NET SDK**, not only the runtime.

## Configuration

1. The configuration file is located at `%APPDATA%\Axiom\settings.toml`.
2. To change the editor theme:
    1. Copy the themes directory from `Resources\Themes\`.
    2. Paste it into `%APPDATA%\Themes\`.

Here’s a cleaner and more professional version of your section, with a clear structure and explicit MIT license
attribution added to each courtesy item:

## Acknowledgements & Courtesy

This project makes use of the following open-source libraries and resources.
We gratefully acknowledge their authors and contributors.

1. **[AvalonEdit](http://avalonedit.net)**<br>
   A WPF-based text editor component.<br>
   Created by [Daniel Grunwald](https://github.com/dgrunwald).<br>
   License: MIT License
2. **[Advanced TextMarkers Usage in AvalonEdit](https://github.com/siegfriedpammer/AvalonEditSamples/)**<br>
   Sample implementations demonstrating advanced text marker usage with AvalonEdit.<br>
   Created by [Siegfried Pammer](https://github.com/siegfriedpammer).<br>
   License: MIT License
3. **[WPF Theme Library (WPFDarkTheme)](https://github.com/AngryCarrot789/WPFDarkTheme)**<br>
   A small dark theme library for WPF and Avalonia applications.<br>
   Created by [REghZy](https://github.com/AngryCarrot789).<br>
   License: MIT License

## References

1. **“Understanding Language Server Protocol”**<br>
   GopherCon UK conference session by [Adrian Hesketh](https://github.com/a-h).<br>
   Watch here: <https://youtu.be/EkK8Jxjj95s>