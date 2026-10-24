namespace Tholi.Core;

/// <summary>One local owner; a UI surface never establishes completion.</summary>
public sealed class JourneyCoordinator
{
    public JourneySnapshot Snapshot { get; private set; } =
        new(Guid.Empty, 0, "", JourneyStatus.Idle);

    public JourneySnapshot Start(string destination, DesktopObservation? observation = null)
    {
        var text = destination.Trim();
        if (text.Length is < 1 or > 1000)
            throw new ArgumentException("A destination must contain 1 to 1000 characters.", nameof(destination));
        Snapshot = new(Guid.NewGuid(), Snapshot.Revision + 1, text,
            observation is null ? JourneyStatus.WaitingForObservation : JourneyStatus.Guiding,
            observation?.Id);
        return Snapshot;
    }

    public void Pause()
    {
        if (Snapshot.Status is JourneyStatus.Guiding or JourneyStatus.WaitingForObservation)
            Snapshot = Snapshot with { Status = JourneyStatus.Paused, Revision = Snapshot.Revision + 1 };
    }

    public void Resume(DesktopObservation observation)
    {
        if (Snapshot.Status != JourneyStatus.Paused) return;
        Snapshot = Snapshot with {
            Status = JourneyStatus.Guiding, ObservationId = observation.Id,
            Revision = Snapshot.Revision + 1
        };
    }

    public void Stop()
    {
        Snapshot = Snapshot with {
            Status = JourneyStatus.Stopped, ObservationId = null,
            Revision = Snapshot.Revision + 1
        };
    }

    public bool ApplyVerifiedOutcome(Guid journeyId, long revision, Guid observationId, bool outcomeObserved)
    {
        if (!outcomeObserved || Snapshot.Id != journeyId || Snapshot.Revision != revision ||
            Snapshot.ObservationId != observationId || Snapshot.Status != JourneyStatus.Guiding)
            return false;
        Snapshot = Snapshot with { Status = JourneyStatus.Completed, Revision = revision + 1 };
        return true;
    }
}
