using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Artillery.Components;

/// <summary>
/// Determines the prototype fired
/// </summary>
[RegisterComponent]
public sealed partial class ArtilleryShellComponent : Component
{
    /// <summary>
    /// Prototype of the ammo to be shot.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype;

    /// <summary>
    /// The sprite path to be used when the charge is loaded.
    /// </summary>
    [DataField]
    public SpriteSpecifier? LoadedSprite;

    /// <summary>
    /// The name of the shell.
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
