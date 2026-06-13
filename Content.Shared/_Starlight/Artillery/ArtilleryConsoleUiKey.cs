using Robust.Shared.Serialization;

namespace Content.Shared.Artillery;

[Serializable, NetSerializable]
public sealed class ArtilleryConsoleUiState : BoundUserInterfaceState
{
    public bool RearBreechOpen;
    public bool ForwardBreechOpen;
    public bool ChargeValid;
    public NetEntity? RearBreechContains;
    public NetEntity? ForwardBreechContains;
    public bool Assembled;
    public bool CanFire;
}

[Serializable, NetSerializable]
public enum ArtilleryConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ArtilleryToggleForwardBreechMessage : BoundUserInterfaceMessage
{}

[Serializable, NetSerializable]
public sealed class ArtilleryToggleRearBreechMessage : BoundUserInterfaceMessage
{}

[Serializable, NetSerializable]
public sealed class ArtilleryFireMessage : BoundUserInterfaceMessage
{}

[Serializable, NetSerializable]
public sealed class ArtilleryFireCall
{}
