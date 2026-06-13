using Robust.Client.UserInterface;
using Content.Shared.Artillery;

namespace Content.Client._Starlight.Artillery.UI;

/// <summary>
/// Initializes a <see cref="ArtilleryConsoleWindow"/> and handles updates to it.
/// </summary>
public sealed class ArtilleryConsoleBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private ArtilleryConsoleWindow? _window;

    public ArtilleryConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) => IoCManager.InjectDependencies(this);

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ArtilleryConsoleWindow>();

        _window.OnFirePressed += () => SendMessage(new ArtilleryFireMessage());
        _window.OnToggleForwardPressed += () => SendMessage(new ArtilleryToggleForwardBreechMessage());
        _window.OnToggleRearPressed += () => SendMessage(new ArtilleryToggleRearBreechMessage());
        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not ArtilleryConsoleUiState consoleState)
            return;

    _window?.UpdateState(consoleState);
    }
}
