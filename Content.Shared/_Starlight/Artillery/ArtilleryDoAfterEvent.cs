using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared.Containers.ItemSlots;

namespace Content.Shared.Artillery;

[Serializable, NetSerializable]
public sealed partial class ArtilleryLoadingDoAfterEvent : SimpleDoAfterEvent
{
    public ItemSlot TargetSlot {get;}
    public NetEntity TargetEnt {get;}

    public ArtilleryLoadingDoAfterEvent(ItemSlot targetSlot, NetEntity targetEnt)
    {
        TargetSlot = targetSlot;
        TargetEnt = targetEnt;
    }
}
