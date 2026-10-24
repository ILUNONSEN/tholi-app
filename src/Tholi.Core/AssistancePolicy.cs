namespace Tholi.Core;

/// <summary>Fail-closed policy for a future native executor. This class performs no input.</summary>
public static class AssistancePolicy
{
    public static AssistanceDecision Evaluate(
        AssistanceRequest request, ConsentGrant? consent,
        JourneySnapshot journey, DesktopObservation observation, DateTimeOffset now)
    {
        if (journey.Status != JourneyStatus.Guiding)
            return new(false, "The journey is not actively guiding.");
        if (request.JourneyId != journey.Id || request.JourneyRevision != journey.Revision ||
            request.ObservationId != journey.ObservationId || request.ObservationId != observation.Id)
            return new(false, "The target belongs to an older journey or observation.");
        if (consent is null || consent.Request != request || consent.ExpiresAt <= now)
            return new(false, "Current consent for this exact action is required.");

        var control = observation.Controls.FirstOrDefault(c => c.Id == request.TargetId);
        if (control is null || !control.Visible || !control.Enabled)
            return new(false, "The target is unavailable.");

        var permitted = (request.Kind, control.Effect) switch {
            (AssistanceKind.RevealMenu, ControlEffect.Reveal) => true,
            (AssistanceKind.SelectOption, ControlEffect.Preference) => true,
            (AssistanceKind.TypeText, ControlEffect.TextInput) => !string.IsNullOrWhiteSpace(request.Text),
            _ => false
        };
        return permitted
            ? new(true, "The bounded action has current consent.")
            : new(false, "This action remains under direct user control.");
    }
}
