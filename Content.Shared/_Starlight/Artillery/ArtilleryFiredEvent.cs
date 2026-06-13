using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Shared.Artillery;

[Serializable, NetSerializable]
public sealed class ArtilleryFiredEvent (MapCoordinates centre, NetEntity barrelEnt, float intensity, SoundSpecifier soundRumble)
{
    public NetEntity BarrelEnt {get;} = barrelEnt;
    public MapCoordinates Centre {get;} = centre;

    public float Intensity {get;} = intensity;

    public SoundSpecifier SoundRumble {get;} = soundRumble;
}
