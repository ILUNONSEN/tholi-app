# Tholi desktop scaffold

A .NET 9 and Avalonia starter for Windows, macOS, and Linux, inspired by the Tholi browser product. The interface uses Tholi's original logo, icon, and shared cream, jade, sage, and amber palette.

## Run

Install a stable .NET 9 SDK from the 9.0.3xx feature band or later (Visual Studio 2022 17.14 includes a compatible SDK), then run from this directory:

```sh
dotnet restore Tholi.sln
dotnet build Tholi.sln -c Release
dotnet run --project src/Tholi.Desktop
dotnet test Tholi.sln -c Release
```

`global.json` selects a stable .NET 9 SDK, with a minimum of 9.0.300. All projects target `net9.0` through `Directory.Build.props`. A .NET 10 SDK is not required. When using Visual Studio, select **Tholi.Desktop** as the startup project.

If F5 reports that a class library cannot be started, expand **src** in Solution Explorer, right-click **Tholi.Desktop**, and choose **Set as Startup Project**, then press F5 again. The desktop project already has `OutputType` set to `WinExe`; Core and Platform are libraries.

The solution lists Desktop as its first project and includes a shared **Tholi Desktop** launch profile in `Tholi.slnLaunch`. Visual Studio can retain an earlier startup selection in local user settings, so an already-open solution may still need the manual selection above. See [Visual Studio startup profiles](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects).

In the app, select **Try: Open display settings**, then **Guide me**. Open Settings and Display in the contained practice workspace. You can ask Tholi to open the practice menu; the action requires a separate consent decision. Pause and Stop invalidate pending consent.

Other destinations remain waiting for a desktop observation. The scaffold does not manufacture routes for applications it cannot observe.

## Projects

| Project | Responsibility |
| --- | --- |
| Tholi.Desktop | Avalonia shell, Guide, Conversation, Settings, and isolated practice. |
| Tholi.Core | Journey state, revision boundaries, contracts, and consent policy. |
| Tholi.Platform | Native adapter boundary, currently reporting all capabilities as unconnected. |
| Tholi.Agent | Independently runnable per-user background host; future journey and OS ownership. |
| Tholi.Runtime | Independently runnable local inference/storage host; future cloud transport. |
| Tholi.Core.Tests | Regressions for stale outcomes, paused/stopped journeys, and exact consent. |

Run the two background hosts separately:

```sh
dotnet run --project src/Tholi.Agent
dotnet run --project src/Tholi.Runtime
```

Use Ctrl+C to stop each console host. For a short capability check that exits:

```sh
dotnet run --project src/Tholi.Agent -- --check
dotnet run --project src/Tholi.Runtime -- --check
```

## Implemented and pending

The shell, local practice, state transitions, consent policy, and host entry points are implemented. The practice owns its own example state and never reads the real desktop.

Native UI Automation / AXUIElement / AT-SPI adapters, overlays, real input, authenticated local IPC, on-device models, cloud accounts, startup registration, installers, and service supervision remain pending. Closing the current preview window exits that UI process. The separately launched hosts remain independent, but the preview journey is not connected to them or persisted across a UI restart.

Do not infer working background journey continuity from the presence of host projects. Connect the interface to the agent over authenticated local IPC before claiming that behavior.

## Product boundaries

Guidance is the primary experience. Limited assistance requires explicit, narrow consent. The scaffold ships no native input executor. A future executor must consume consent once, revalidate the actual target immediately before input, respect user interruption, and verify outcomes afterward.

Core consent binds the exact request, target, text, journey revision, observation, and expiry. Unsupported and consequential effects are refused. Model output supplies proposals; the session agent retains interaction authority.

## Assets

The original logo and icon were copied unchanged from the local `Desktop/tholi/Media` folder. See [asset provenance](assets/provenance.json) for hashes and [third-party notices](THIRD_PARTY_NOTICES.md) for attribution. No model weights or web runtime bundles are copied into this starter.

The main Tholi repository is https://github.com/Sammieblz/tholi, inspected at commit `231818dfff99dd7eda48be90973625cc08cb6d4d`. Its TypeScript runtime is not embedded here.

## Platform validation

The projects share portable .NET contracts. Building on Windows does not certify macOS or Linux native behavior. Test permissions, packaging, overlays, display scaling, and lifecycle separately on each declared target.

A native feasibility sequence is described in [architecture](docs/scaffold-architecture.md).
