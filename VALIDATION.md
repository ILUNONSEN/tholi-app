# Scaffold validation

Validated October 24, 2026 in this Windows checkout using the installed .NET SDK 9.0.304. Visual Studio Community 2022 17.14.13 is installed; the IDE itself was not exercised during these checks.

## Toolchain correction

The initial scaffold targeted `net10.0` and required SDK 10.0.100, which was not installed. All projects now target `net9.0`. `global.json` selects a stable .NET 9 SDK at or above feature band 9.0.300, matching the installed Visual Studio toolchain.

## Passed

- NuGet restore for all six projects.
- Full Release solution build, including Avalonia XAML compilation: zero warnings, zero errors.
- xUnit discovery and execution: all eight tests passed, with none skipped. These cover missing observations, stale journey outcomes, pause/stop invalidation, outcome verification, exact consent, changed requests, expiry, and consequential actions.
- Agent and runtime `--check` entry points executed successfully. They report native capabilities as unconnected and no loaded model, cloud connection, or runtime listener.

Commands used:

```sh
dotnet --version
dotnet restore Tholi.sln --disable-parallel -m:1
dotnet build Tholi.sln -c Release --no-restore -m:1
dotnet test tests/Tholi.Core.Tests/Tholi.Core.Tests.csproj -c Release --no-build --no-restore -m:1
dotnet src/Tholi.Agent/bin/Release/net9.0/Tholi.Agent.dll --check
dotnet src/Tholi.Runtime/bin/Release/net9.0/Tholi.Runtime.dll --check
```

Single-node MSBuild was used because the initial parallel restore exited unsuccessfully without reporting an error in this execution environment.

## Pending

Real UI rendering, continuous background host execution, and macOS/Linux runtime behavior remain unverified. The capability checks do not demonstrate native desktop navigation, agent/UI communication, or background journey continuity.

Native adapters, overlays, real input, authenticated IPC, local models, cloud transport, installers, startup registration, and service supervision remain unimplemented. See SCAFFOLD.md and docs/scaffold-architecture.md.

## Earlier scaffold checks

The original delivery report recorded ten offline checks using an isolated .NET 10 SDK and byte-for-byte artwork provenance. Those checks were not rerun as part of this toolchain correction. This checkout's successful restore, build, and xUnit execution supersede the earlier report's NuGet-blocked status. Artwork hashes remain in assets/provenance.json.
