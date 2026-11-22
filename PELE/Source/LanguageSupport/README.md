# P.E.L.E language rendering source (1.0.8)

This directory contains the source used for the additional Thai, Hindi and Arabic language support included in P.E.L.E 1.0.8:

- `LanguageSupport.cs` registers the languages and their flags, and coordinates text presentation in the game.
- `LanguageRenderWorker.cs` runs the local `ShapeText.exe` worker and communicates with it through standard input and output. It substitutes a marker if a requested text sprite is unavailable.
- `ShapeText/ShapeText.cs` and `ShapeText/RuntimeShape.cs` generate images of text using Windows text rendering and the fonts bundled with the package. The worker uses ICU for Thai word boundaries when available, with a fallback when ICU is unavailable.

The executable is a local text-rendering helper, not an installer. The three font licenses are included alongside the fonts in the distribution ZIP. The existing `R2API.Language-Source` directory contains the language API fork; the packaging also includes separate components not built by this directory.

## Build the text renderer on Windows

From a PowerShell window in `ShapeText`, using the .NET Framework 4 compiler installed with Windows:

```powershell
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$wpf = Join-Path $framework 'WPF'
& (Join-Path $framework 'csc.exe') /nologo /target:exe /optimize+ /out:ShapeText.exe `
  "/reference:$wpf/PresentationCore.dll" "/reference:$wpf/PresentationFramework.dll" `
  "/reference:$wpf/WindowsBase.dll" "/reference:$framework/System.Xaml.dll" `
  ShapeText.cs RuntimeShape.cs
```

The runtime expects the packaged `fonts` directory beside `ShapeText.exe`. The compiler produces assembly-specific metadata, so a rebuilt executable is not necessarily byte-identical to the release. The compiled managed instructions for the two source classes were compared against the 1.0.8 executable, ignoring generated private-implementation identifiers; this is not a bit-for-bit reproducible build claim.

## Build the game plugin

`LanguageSupport.csproj` targets .NET Standard 2.1 and requires references from a local Risk of Rain 2 installation and a BepInEx profile with P.E.L.E installed. Set `GameManaged` and, if necessary, `ProfileBepInEx` to those local directories when building with `dotnet build`. The project does not deploy the result automatically. Do not include game assemblies in redistributions.
