# WARP

This document explains how to work with the Angor Avalonia application in this fork using Warp.

## Solutions
- Legacy: `src/Angor.sln` (Blazor/Tauri; kept for compatibility – not the focus here).
- New: `src/Angor/Avalonia/Angor.Avalonia.sln` (primary). The application is AngorApp for Desktop and Mobile (Android).

## SDK and global.json
- .NET SDK: 9.0.x.
- Important: the Avalonia solution has its own `global.json` at `src/Angor/Avalonia/global.json` (SDK 9). Do not rely on any repository-root SDK settings for Avalonia builds.

## Build and run
- Build everything (recommended from the Avalonia folder to pick the local global.json):
  - `dotnet build src/Angor/Avalonia/Angor.Avalonia.sln -c Debug`

- Desktop (Linux/macOS/Windows):
  - Run: `dotnet run --project src/Angor/Avalonia/AngorApp.Desktop -c Debug -- --profile=Default`
  - Publish: `dotnet publish src/Angor/Avalonia/AngorApp.Desktop -c Release -o out/desktop`

- Android:
  - Prerequisite (once): `dotnet workload install android`
  - Build APK: `dotnet publish src/Angor/Avalonia/AngorApp.Android -c Release -f net9.0-android -p:AndroidPackageFormat=apk -o out/android`
  - The project defaults to `android-arm64` in Release.

## Testing
- Unit tests (contexts):
  - `dotnet test src/Angor/Avalonia/Angor.Contexts.Wallet.Tests`
  - `dotnet test src/Angor/Avalonia/Angor.Contexts.Funding.Tests`
- You can also test the whole solution: `dotnet test src/Angor/Avalonia/Angor.Avalonia.sln`

## Architecture and conventions
- MVVM strictly; near 0% code-behind. Views in Avalonia XAML; logic lives in ViewModels.
- ReactiveUI is used only in ViewModels, with its Source Generators (ReactiveUI.SourceGenerators) enabled. Prefer reactive over imperative code.
- Prefer functional style with CSharpFunctionalExtensions (Result/Maybe, etc.).
- Bounded contexts: domain is split into two fully separated contexts:
  - Wallet: `src/Angor/Avalonia/Angor.Contexts.Wallet/*`
  - Funding: `src/Angor/Avalonia/Angor.Contexts.Funding/*`
  - Cross-cutting and integration: `Angor.Contexts.CrossCutting`, `Angor.Contexts.Integration.WalletFunding`.
- UI composition and DI:
  - Composition root: `src/Angor/Avalonia/AngorApp/Composition/CompositionRoot.cs` (Microsoft.Extensions.DependencyInjection, Serilog, Zafiro navigation services).
  - Sections and navigation registered under `AngorApp/Composition/Registrations/*` and `AngorApp/Sections/*`.
  - Profiles: app supports `--profile=<name>`; default is `Default`.
- Packages are centrally managed via `src/Angor/Avalonia/Directory.Packages.props` (keep Avalonia package versions in sync).
- Additional UI libs: Zafiro.Avalonia (+ Generators), Humanizer, Icons (Projektanker FontAwesome/MaterialDesign), AsyncImageLoader, PanAndZoom, QRCoder.

### Reactive guidelines
- Use ReactiveUI and WhenAny* helpers; avoid putting logic in `Subscribe`.
- Use ReactiveUI Validations when applicable.
- Do not suffix async methods with `Async`.
- Do not use underscores for private fields.

## CI/CD
- Azure Pipelines config at `src/Angor/Avalonia/azure-pipelines.yml`:
  - Installs .NET Runtime 8 and SDK 9, installs Android workload and `DotnetDeployer.Tool`.
  - Releases are produced via `dotnetdeployer github release` against `Angor.Avalonia.sln`, targeting Desktop and Android (requires signing params via pipeline variables).

## Project entry points
- Desktop: `src/Angor/Avalonia/AngorApp.Desktop/Program.cs` launches `AngorApp`.
- Android: `src/Angor/Avalonia/AngorApp.Android/MainActivity.cs` with manifest at `.../Properties/AndroidManifest.xml`.
- Shared app project: `src/Angor/Avalonia/AngorApp/AngorApp.csproj` targeting `net9.0`.

## Troubleshooting
- Build uses the Avalonia-level `global.json`. If you see SDK/TFM mismatches, ensure commands run from `src/Angor/Avalonia` or that SDK 9 is selected.
- For Android, ensure the Android workload is installed and Java/Android tooling is available in your environment when deploying to devices/emulators.
