using Tholi.Core;
using Xunit;

namespace Tholi.Core.Tests;

public sealed class JourneyTests
{
    private static DesktopObservation Observe() => new(Guid.NewGuid(), "example", "window", []);

    [Fact]
    public void DestinationWithoutNativeEvidenceWaits()
    {
        var coordinator = new JourneyCoordinator();
        Assert.Equal(JourneyStatus.WaitingForObservation, coordinator.Start("Open settings").Status);
    }

    [Fact]
    public void ReplacedJourneyRejectsAnOldOutcome()
    {
        var coordinator = new JourneyCoordinator();
        var observation = Observe();
        var old = coordinator.Start("Open settings", observation);
        coordinator.Start("Open help", observation);
        Assert.False(coordinator.ApplyVerifiedOutcome(old.Id, old.Revision, observation.Id, true));
        Assert.Equal(JourneyStatus.Guiding, coordinator.Snapshot.Status);
    }

    [Fact]
    public void PausedAndStoppedJourneysRejectDelayedCompletion()
    {
        var coordinator = new JourneyCoordinator();
        var observation = Observe();
        var snapshot = coordinator.Start("Open settings", observation);
        coordinator.Pause();
        Assert.False(coordinator.ApplyVerifiedOutcome(snapshot.Id, snapshot.Revision, observation.Id, true));
        coordinator.Resume(observation);
        snapshot = coordinator.Snapshot;
        coordinator.Stop();
        Assert.False(coordinator.ApplyVerifiedOutcome(snapshot.Id, snapshot.Revision, observation.Id, true));
        Assert.Null(coordinator.Snapshot.ObservationId);
    }

    [Fact]
    public void DispatchWithoutObservedOutcomeCannotComplete()
    {
        var coordinator = new JourneyCoordinator();
        var observation = Observe();
        var snapshot = coordinator.Start("Open settings", observation);
        Assert.False(coordinator.ApplyVerifiedOutcome(snapshot.Id, snapshot.Revision, observation.Id, false));
    }
}
