using Tholi.Core;

namespace Tholi.Platform;

/// <summary>Explicit capability boundary until a native adapter is implemented.</summary>
public sealed class UnconnectedDesktopAdapter : IDesktopAdapter
{
    public DesktopCapabilities Capabilities { get; } = new(
        OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsMacOS() ? "macOS" : "Linux",
        false, false, false, false,
        "Native observation, capture, overlays, and input are not connected in this scaffold.");

    public ValueTask<DesktopObservation?> ObserveAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<DesktopObservation?>(null);
    }
}
