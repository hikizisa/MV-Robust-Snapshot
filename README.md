# MV-Robust-Snapshot

A standalone plugin for **Mapset Verifier** that replaces the default `TimingPoints` snapshot translator with a robust alignment implementation using Dynamic Time Warping (DTW) and RANSAC.

## Features

- **Robust Timing Alignment**: Correctly aligns timing changes during map edits even with section-wide time-shifts (e.g. mp3 offset adjustments).
- **Sub-Snap Jitter Filtering**: Distinguishes between global section shifts and minor local timing adjustments.
- **Standalone Design**: Built as a plugin DLL that can be compiled and deployed separately. It overrides the default timing translator at runtime without requiring edits to the core Mapset Verifier installation.

---

## How It Works

Mapset Verifier's custom checks are loaded dynamically at startup. This plugin extends `GeneralCheck` so that it is instantiated during the custom checks loading phase. 

Upon instantiation, it performs a one-time Reflection hook that:
1. Resolves and initializes Mapset Verifier's static `TranslatorRegistry`.
2. Locates the private list of active translators.
3. Removes the default `TimingTranslator` handling the `TimingPoints` section.
4. Registers `CustomTimingTranslator` in its place.

---

## Build Guide

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Compilation
Clone the repository and build the project using the .NET CLI:
```bash
dotnet build
```

This will produce the compiled assembly `MapsetVerifier.Plugin.CustomSnapshots.dll` in the output folder `bin/Debug/net9.0/`.

---

## Installation & Usage

1. Copy the compiled `MapsetVerifier.Plugin.CustomSnapshots.dll` file.
2. Paste it into your Mapset Verifier Custom Checks folder:
   - **Windows**: `%APPDATA%\Mapset Verifier Externals\CustomChecks\`
   - **Linux**: `~/.local/share/Mapset Verifier Externals/CustomChecks/`

The bundled `lib/` references target the older .NET 9 host. Build against the assemblies and .NET target of the installed Mapset Verifier version before deploying: a host API change such as `TimingLine.SvMult` changing from `float` to `double` is binary incompatible even when the assembly names match.

For a .NET 10 installation, set `HostAssemblyDirectory` to a directory containing the matching host DLLs (extract them first if the host is published as a single executable):

```powershell
dotnet restore -p:TargetFramework=net10.0
dotnet build -c Release --no-restore -p:TargetFramework=net10.0 -p:HostAssemblyDirectory="C:\path\to\matching-host-assemblies" -p:DeployPlugin=true
```

Automatic installation is opt-in through `DeployPlugin=true`, so a normal development build cannot overwrite an installed plugin with an incompatible build.

3. Restart Mapset Verifier. The application logs will display:
   ```text
   [CustomSnapshotsPlugin] Initializing snapshot translator replacement hook...
   [CustomSnapshotsPlugin] Removed 1 default TimingPoints translator(s).
   [CustomSnapshotsPlugin] Registered custom TimingPoints translator successfully.
   ```
4. The application will now use the robust timing comparison logic automatically when translating snapshot differences!
