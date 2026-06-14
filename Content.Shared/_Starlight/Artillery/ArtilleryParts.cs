using Robust.Shared.Serialization;

namespace Content.Shared.Artillery;

[Serializable, NetSerializable]
public enum ArtilleryParts : byte
{
    RearBreech,
    ForwardBreech,
    RecoilAssembly,
    PrimaryBarrel,
    SecondaryBarrel
};

[Serializable, NetSerializable]
public enum BreechType : byte
{
    RearBreech,
    ForwardBreech
};

[Serializable, NetSerializable]
public enum ArtilleryBreechVisuals
{
    State,
    LoadedSprite,
    OpenLayer,
    LoadedLayer,
    LoadedState
}

[Serializable, NetSerializable]
public enum ArtilleryBreechState
{
    Closed,
    Open,
    Opening,
    Closing
}
