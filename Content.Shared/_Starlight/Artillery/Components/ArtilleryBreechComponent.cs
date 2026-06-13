using Robust.Shared.GameStates;
using Robust.Shared.Containers;
using Robust.Shared.Audio;

namespace Content.Shared.Artillery.Components;

/// <summary>
/// Marks this entity as a valid breach and sets the side
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArtilleryBreechComponent : Component
{
    /// <summary>
    /// If the loading hatch is open
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public ArtilleryBreechState State { get; set; }

    /// <summary>
    /// Allows entities to be loaded after the doafter completes.
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Time until next state change.
    /// </summary>
    [DataField]
    public TimeSpan? NextStateChange;

    /// <summary>
    /// The length of the breech opening animation.
    /// </summary>
    [DataField]
    public TimeSpan OpeningAnimationTime = TimeSpan.FromSeconds(0.9);

    /// <summary>
    /// The length of the breech closing animation.
    /// </summary>
    [DataField]
    public TimeSpan ClosingAnimationTime = TimeSpan.FromSeconds(0.9);

    /// <summary>
    /// Which breech it is (RearBreech/ForwardBreech)
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public BreechType Side;

    [ViewVariables]
    public ContainerSlot BreechSlot = default!;

    /// <summary>
    /// Container slot ID
    /// </summary>
    public const string BreechSlotId = "breech_slot";

    /// <summary>
    /// The sound to use when the hatch is opened.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier? SoundOpen = default!;

    /// <summary>
    /// The sound to use when the hatch is closed.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier? SoundClose = default!;

    /// <summary>
    /// How many seconds it takes to load an item into the breech.
    /// </summary>
    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(5.0);

    /// <summary>
    /// How long between being toggled to wait before allowing it to happen again.
    /// </summary>
    [DataField]
    public TimeSpan ToggleCooldown = TimeSpan.FromSeconds(1.0);

    [AutoNetworkedField]
    public TimeSpan LastToggled;
}
