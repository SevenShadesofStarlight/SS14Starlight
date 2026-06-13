using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using Robust.Shared.Audio;

namespace Content.Shared.Artillery.Components;

/// <summary>
/// Determines the casing used and the intensity of the explosion
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArtilleryChargeComponent : Component
{
    /// <summary>
    /// Prototype of the casing this is replaced by after being fired.
    /// </summary>
    [DataField]
    public EntProtoId Casing = "ArtilleryPropellantEmpty";

    /// <summary>
    /// If the charge has been fired.
    /// </summary>
    [DataField]
    public bool Spent;

    /// <summary>
    /// Intensity of the explosion, for recoil and global sound.
    /// </summary>
    [DataField]
    public float Intensity = 4000;

    /// <summary>
    /// The sound to use when the gun is fired.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundGunshot = new SoundPathSpecifier("/Audio/Effects/explosion6.ogg");

    /// <summary>
    /// The sound to use for the long distance rumble.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundRumble = new SoundPathSpecifier("/Audio/Effects/explosionfar.ogg");

    /// <summary>
    /// The sprite path to be used when the charge is loaded.
    /// </summary>
    [DataField]
    public SpriteSpecifier? LoadedSprite;

    /// <summary>
    /// The name of the charge or casing.
    /// </summary>
    [DataField(required: true)]
    public string Name;

    /// <summary>
    /// The suffix for the loaded state, defaults to "loaded".
    /// </summary>
    [DataField]
    public string LoadedSuffix = "loaded";

    /// <summary>
    /// The suffix for the loading state, defaults to "loading".
    /// </summary>
    [DataField]
    public string LoadingSuffix = "loading";

    /// <summary>
    /// The suffix for the unloading state, defaults to "unloading".
    /// </summary>
    [DataField]
    public string UnloadingSuffix = "unloading";
}
