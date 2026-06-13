using Robust.Shared.GameStates;

namespace Content.Shared.Artillery.Components;

/// <summary>
/// Marks this entity as a valid secondary barrel,
/// sets the offset for calculating where the projectile is fired from,
/// and determines the muzzle flash proto used when the gun is fired.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArtillerySecondaryBarrelComponent : Component
{
    /// <summary>
    /// Distance from the origin of the entity to the end of the barrel
    /// (not the total length)
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float BarrelLength;

    /// <summary>
    /// Muzzle flash effect to use
    /// </summary>
    [DataField, AutoNetworkedField]
    public string MuzzleFlash = "ArtilleryMuzzleFlashEffect";
}
