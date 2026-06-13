using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Artillery.Components;

/// <summary>
/// Marks this entity as a valid artillery body,
/// and sets the sound to use when firing
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArtilleryBodyComponent : Component
{
    /// <summary>
    /// The sound to use when the gun is fired.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundGunshot = new SoundPathSpecifier("/Audio/_Starlight/Weapons/Shuttleguns/uaf_cyrexa_452.ogg");

    /// <summary>
    /// The proto ID of the "Artillery Data" source port
    /// </summary>
    [DataField("artilleryDataPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string ArtilleryDataPort = "ArtilleryDataSender";
}
