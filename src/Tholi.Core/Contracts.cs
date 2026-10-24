namespace Tholi.Core;

public enum JourneyStatus { Idle, WaitingForObservation, Guiding, Paused, Completed, Stopped }
public enum AssistanceKind { RevealMenu, SelectOption, TypeText, Commit }
public enum ControlEffect { Reveal, Preference, TextInput, Consequential, Unknown }

public sealed record ControlEvidence(
    string Id, string Name, ControlEffect Effect, bool Visible = true, bool Enabled = true);

public sealed record DesktopObservation(
    Guid Id, string ApplicationId, string WindowId, IReadOnlyList<ControlEvidence> Controls);

public sealed record JourneySnapshot(
    Guid Id, long Revision, string Destination, JourneyStatus Status, Guid? ObservationId = null);

public sealed record AssistanceRequest(
    Guid Id, Guid JourneyId, long JourneyRevision, Guid ObservationId,
    string TargetId, AssistanceKind Kind, string Description, string? Text = null);

public sealed record ConsentGrant(AssistanceRequest Request, DateTimeOffset ExpiresAt);

public sealed record AssistanceDecision(bool Allowed, string Reason);

public sealed record DesktopCapabilities(
    string Platform, bool Accessibility, bool Capture, bool Overlay, bool Input,
    string Detail);

public interface IDesktopAdapter
{
    DesktopCapabilities Capabilities { get; }
    ValueTask<DesktopObservation?> ObserveAsync(CancellationToken cancellationToken);
}

public interface IOverlayPresenter
{
    ValueTask ShowAsync(DesktopObservation observation, string targetId, CancellationToken cancellationToken);
    ValueTask HideAsync(CancellationToken cancellationToken);
}

public interface IInferenceProvider
{
    ValueTask<string?> SuggestAsync(string destination, DesktopObservation observation, CancellationToken cancellationToken);
}
