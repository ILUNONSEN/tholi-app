using Tholi.Core;
using Xunit;

namespace Tholi.Core.Tests;

public sealed class AssistanceTests
{
    private readonly DateTimeOffset now = DateTimeOffset.Parse("2026-10-24T12:00:00Z");

    private static (AssistanceRequest Request, JourneySnapshot Journey, DesktopObservation Observation) Example()
    {
        var observation = new DesktopObservation(Guid.NewGuid(), "example", "window",
            [new("menu", "Settings", ControlEffect.Reveal)]);
        var journey = new JourneyCoordinator().Start("Open settings", observation);
        var request = new AssistanceRequest(Guid.NewGuid(), journey.Id, journey.Revision,
            observation.Id, "menu", AssistanceKind.RevealMenu, "Open settings menu");
        return (request, journey, observation);
    }

    [Fact]
    public void MissingConsentCannotAuthorizeInput()
    {
        var (request, journey, observation) = Example();
        Assert.False(AssistancePolicy.Evaluate(request, null, journey, observation, now).Allowed);
    }

    [Fact]
    public void ExactConsentPermitsOnlyTheCurrentBoundAction()
    {
        var (request, journey, observation) = Example();
        var consent = new ConsentGrant(request, now.AddSeconds(20));
        Assert.True(AssistancePolicy.Evaluate(request, consent, journey, observation, now).Allowed);
        Assert.False(AssistancePolicy.Evaluate(request, consent, journey,
            observation with { Id = Guid.NewGuid() }, now).Allowed);
        Assert.False(AssistancePolicy.Evaluate(request, consent,
            journey with { Status = JourneyStatus.Stopped }, observation, now).Allowed);
    }

    [Fact]
    public void ChangedActionCannotReuseConsent()
    {
        var (request, journey, observation) = Example();
        var consent = new ConsentGrant(request, now.AddSeconds(20));
        Assert.False(AssistancePolicy.Evaluate(request with { Text = "unexpected text" },
            consent, journey, observation, now).Allowed);
    }

    [Fact]
    public void ExpiredConsentAndConsequentialActionsAreRefused()
    {
        var (request, journey, observation) = Example();
        Assert.False(AssistancePolicy.Evaluate(request, new(request, now),
            journey, observation, now).Allowed);
        request = request with { Kind = AssistanceKind.Commit };
        Assert.False(AssistancePolicy.Evaluate(request, new(request, now.AddSeconds(20)),
            journey, observation, now).Allowed);
    }
}
