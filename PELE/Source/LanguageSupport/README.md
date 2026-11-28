# P.E.L.E. language presentation

The game plugin for P.E.L.E. 1.0.10 handles complex text presentation, fallback fonts, the language selector and HUD updates. It exchanges requests with the local ShapeText executable through standard input and output. See the [build instructions](../README.md) for the plugin.

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

The runtime expects the packaged `fonts` directory beside `ShapeText.exe`. The compiler produces assembly-specific metadata, so a rebuilt executable is not necessarily byte-identical to the release. The compiled managed instructions for the two source classes were compared against the 1.0.8 executable, ignoring generated private-implementation identifiers; the 1.0.10 ZIP ships that same executable.

