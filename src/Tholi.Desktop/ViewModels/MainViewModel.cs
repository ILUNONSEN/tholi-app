using Tholi.Core;

namespace Tholi.Desktop.ViewModels;

/// <summary>Controlled practice, isolated from native desktop access and the background agent.</summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly JourneyCoordinator journey = new();
    private DesktopObservation observation = CreateObservation();
    private string destination = "";
    private string title = "Where would you like to go?";
    private string instruction = "Choose the practice destination below, or describe a task for a future connected desktop.";
    private string notice = "Practice stays inside this window. Your desktop is not being observed.";
    private string section = "Guide";
    private bool menuOpen;
    private bool displayOpen;
    private bool consentVisible;
    private AssistanceRequest? pendingRequest;

    public string Destination { get => destination; set { destination = value; Changed(); StartCommand.Refresh(); } }
    public string Title => title;
    public string Instruction => instruction;
    public string Notice => notice;
    public string Section => section;
    public bool ShowGuide => section == "Guide";
    public bool ShowConversation => section == "Conversation";
    public bool ShowSettings => section == "Settings";
    public bool MenuOpen => menuOpen;
    public bool DisplayOpen => displayOpen;
    public bool ConsentVisible => consentVisible;
    public bool CanNavigate => journey.Snapshot.Status == JourneyStatus.Guiding;
    public string Status => journey.Snapshot.Status.ToString();
    public string Conversation => string.IsNullOrWhiteSpace(destination)
        ? "Tell Tholi where you want to go. Your current request and observed steps will appear here."
        : $"Destination: {destination}\n\n{title}\n{instruction}\n\n{notice}";

    public RelayCommand StartCommand { get; }
    public RelayCommand SampleCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand OpenMenuCommand { get; }
    public RelayCommand OpenDisplayCommand { get; }
    public RelayCommand RequestHelpCommand { get; }
    public RelayCommand ApproveCommand { get; }
    public RelayCommand DeclineCommand { get; }
    public RelayCommand GuideCommand { get; }
    public RelayCommand ConversationCommand { get; }
    public RelayCommand SettingsCommand { get; }

    public MainViewModel()
    {
        StartCommand = new(Start, () => !string.IsNullOrWhiteSpace(destination));
        SampleCommand = new(() => Destination = "Open display settings");
        PauseCommand = new(TogglePause, () => journey.Snapshot.Status is JourneyStatus.Guiding or JourneyStatus.Paused);
        StopCommand = new(Stop, () => journey.Snapshot.Status is JourneyStatus.Guiding or JourneyStatus.Paused or JourneyStatus.WaitingForObservation);
        OpenMenuCommand = new(OpenMenu, () => CanNavigate && !menuOpen);
        OpenDisplayCommand = new(OpenDisplay, () => CanNavigate && menuOpen && !displayOpen);
        RequestHelpCommand = new(RequestHelp, () => CanNavigate && !menuOpen);
        ApproveCommand = new(Approve);
        DeclineCommand = new(() => { pendingRequest = null; consentVisible = false; Refresh(); });
        GuideCommand = new(() => SwitchSection("Guide"));
        ConversationCommand = new(() => SwitchSection("Conversation"));
        SettingsCommand = new(() => SwitchSection("Settings"));
    }

    private void Start()
    {
        pendingRequest = null;
        consentVisible = false;
        menuOpen = displayOpen = false;
        observation = CreateObservation();
        var practice = string.Equals(destination.Trim(), "Open display settings", StringComparison.OrdinalIgnoreCase);
        journey.Start(destination, practice ? observation : null);
        title = practice ? "Open the Settings menu" : "Your destination is ready";
        instruction = practice
            ? "In the practice workspace, select Settings. You make the click, or ask for this one small action."
            : "Connect a native desktop adapter to begin guidance for this destination.";
        notice = practice
            ? "A controlled example. No other application is accessed."
            : "No native adapter is connected. Tholi has not planned a route or completed this task.";
        Refresh();
    }

    private void TogglePause()
    {
        pendingRequest = null;
        consentVisible = false;
        if (journey.Snapshot.Status == JourneyStatus.Paused)
        {
            observation = observation with { Id = Guid.NewGuid() };
            journey.Resume(observation);
        }
        else journey.Pause();
        notice = journey.Snapshot.Status == JourneyStatus.Paused
            ? "Practice is paused. Use Pause / resume to continue."
            : "Practice resumed against fresh example evidence.";
        Refresh();
    }

    private void Stop()
    {
        journey.Stop();
        pendingRequest = null;
        consentVisible = false;
        title = "Take your time";
        instruction = "This journey has stopped. Start a new destination whenever you are ready.";
        notice = "No actions are queued.";
        Refresh();
    }

    private void OpenMenu()
    {
        if (!CanNavigate) return;
        pendingRequest = null;
        consentVisible = false;
        menuOpen = true;
        title = "Choose Display";
        instruction = "The practice menu is open. Select Display to reach the requested screen.";
        notice = "Opening a menu is an intermediate step.";
        Refresh();
    }

    private void OpenDisplay()
    {
        if (!CanNavigate || !menuOpen) return;
        displayOpen = true;
        // The practice workspace's selected screen is the outcome oracle.
        var current = journey.Snapshot;
        if (journey.ApplyVerifiedOutcome(current.Id, current.Revision, observation.Id, displayOpen))
        {
            title = "You are in display settings";
            instruction = "The practice workspace now shows the requested Display screen.";
            notice = "Outcome verified in this example only.";
        }
        Refresh();
    }

    private void RequestHelp()
    {
        pendingRequest = new(Guid.NewGuid(), journey.Snapshot.Id, journey.Snapshot.Revision,
            observation.Id, "settings-menu", AssistanceKind.RevealMenu, "Open the practice Settings menu");
        consentVisible = true;
        Refresh();
    }

    private void Approve()
    {
        var request = pendingRequest;
        pendingRequest = null;
        consentVisible = false;
        if (request is null) { Refresh(); return; }
        var decision = AssistancePolicy.Evaluate(request,
            new(request, DateTimeOffset.UtcNow.AddSeconds(20)), journey.Snapshot, observation, DateTimeOffset.UtcNow);
        if (decision.Allowed) OpenMenu();
        else { notice = decision.Reason; Refresh(); }
    }

    private void SwitchSection(string value)
    {
        section = value;
        Changed(nameof(Section)); Changed(nameof(ShowGuide));
        Changed(nameof(ShowConversation)); Changed(nameof(ShowSettings));
    }

    private void Refresh()
    {
        foreach (var property in new[] { nameof(Title), nameof(Instruction), nameof(Notice),
            nameof(Status), nameof(MenuOpen), nameof(DisplayOpen), nameof(ConsentVisible),
            nameof(CanNavigate), nameof(Conversation) }) Changed(property);
        PauseCommand.Refresh(); StopCommand.Refresh(); OpenMenuCommand.Refresh();
        OpenDisplayCommand.Refresh(); RequestHelpCommand.Refresh();
    }

    private static DesktopObservation CreateObservation() => new(
        Guid.NewGuid(), "tholi-practice", "practice-window",
        [new("settings-menu", "Settings", ControlEffect.Reveal),
         new("display-option", "Display", ControlEffect.Preference)]);
}
