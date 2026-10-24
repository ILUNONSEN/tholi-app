# Desktop scaffold architecture

## Product direction

Use .NET for Windows, Linux, and macOS. Keep guidance as the primary interaction and offer small actions with explicit consent. Support a lightweight local backend and local vision/neural inference alongside the shared Tholi cloud services.

## Process ownership

```mermaid
flowchart LR
    UI[Tholi Desktop] <-->|Authenticated local IPC to implement| Agent[Session agent]
    Agent <-->|Local inference contract to implement| Runtime[Local runtime]
    Runtime <-->|Opt in cloud transport to implement| Cloud[Shared Tholi backend]
```

The interface attaches to one agent-owned journey. The agent observes the interactive user's desktop, binds targets, presents guidance, enforces consent, and verifies outcomes. The local runtime hosts models, reusable knowledge, and optional cloud transport.

Current practice is intentionally isolated inside Tholi.Desktop. Host processes are scaffold entry points and have no IPC listener. They are not installed as system services or registered at login.

## Native adapters

| Platform | Planned observation boundary |
| --- | --- |
| Windows | UI Automation, plus Java Access Bridge where an application supports it. |
| macOS | Accessibility APIs and separately authorized screen capture. |
| Linux | AT-SPI and desktop-supported capture/input mechanisms, with X11 and Wayland evaluated separately. |

`UnconnectedDesktopAdapter` reports unsupported capabilities until a real implementation is present. Native application/window identity must remain distinct from browser document identity.

Use current accessibility evidence before OCR or visual estimation. Custom panels and canvases may need a vision fallback or explicit application integration. A desktop overlay can draw around current screen bounds without injecting controls into another application. Revalidate after window movement, scaling, scrolling, dialog changes, and occlusion.

## Consent and verification

`AssistancePolicy` checks exact request equality, current journey revision, current observation, expiry, visibility, enabled state, and supported effects. It never dispatches native input. A future executor also needs single-use consumption, cancellation, fresh native binding, and final risk checks.

The initial policy permits requested menu reveals, preference selections, and non-empty text for a declared text-input target. Credential and sensitive-field classification belongs in the native adapter and executor before typing can be offered. Consequential and unknown effects remain under direct user control.

`JourneyCoordinator` rejects outcomes from replaced, paused, or stopped journeys. Waiting for observation is explicit. Dispatching a click does not complete a journey.

## First native milestone

1. Implement read-only observation for one Windows application and OS settings workflow.
2. Render and invalidate a target overlay, including multiple monitors and display scaling.
3. Verify an outcome from current application evidence.
4. Connect the UI to the agent using authenticated local IPC.
5. Verify closing and terminating only the UI preserves the agent-owned journey.
6. Implement one consented low-risk action with interruption and single-use authorization.
7. Evaluate local OCR or a small ONNX model against measured latency and memory limits.
8. Run early macOS and Linux feasibility checks before promising equivalent capabilities.

## References

- [Tholi navigation architecture](https://github.com/Sammieblz/tholi/blob/main/docs/architecture/navigation.md)
- [Tholi shared UI tokens](https://github.com/Sammieblz/tholi/blob/main/packages/ui/styles/tokens.css)
- [Avalonia desktop platforms](https://docs.avaloniaui.net/docs/supported-platforms)
- [Windows service and interactive-session separation](https://learn.microsoft.com/en-us/windows/desktop/services/interactive-services)
