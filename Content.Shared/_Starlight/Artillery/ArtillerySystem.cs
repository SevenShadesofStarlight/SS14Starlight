using Content.Shared.Machines.EntitySystems;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Artillery.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Machines.Components;
using Content.Shared._Starlight.Camera;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.DeviceLinking;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using Content.Shared.Camera;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Map;
using System.Numerics;
using Robust.Shared.Utility;

namespace Content.Shared.Artillery;
public sealed partial class ArtillerySystem : EntitySystem
{
    #pragma warning disable RA0051, CS0628
    [Dependency] private readonly SharedMultipartMachineSystem _multiPartMachineSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    #pragma warning restore

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtilleryBreechComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<ArtilleryBreechComponent, ArtilleryLoadingDoAfterEvent>(HandleInsert);

        SubscribeLocalEvent<ArtilleryBreechComponent, ItemSlotEjectAttemptEvent>(OnEjectAttempt);
        SubscribeLocalEvent<ArtilleryBreechComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<ArtilleryBreechComponent, EntRemovedFromContainerMessage>(OnEjected);

        SubscribeLocalEvent<ArtilleryBodyComponent, ArtilleryFireCall>(Fire);
    }
#region Breech
    private void OnInsertAttempt(Entity<ArtilleryBreechComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (!ent.Comp.IsLoading)
            args.Cancelled = true;
        else
            ent.Comp.IsLoading = false;

        if (ent.Comp.State != ArtilleryBreechState.Open)
            return;

        if  (args.User is null)
            return;

        var netEnt = GetNetEntity(args.Item);
        // Start a doafter for loading the shell/charge.
        var doAfterEvent = new DoAfterArgs(EntityManager,
            args.User.Value,
            ent.Comp.DoAfter,
            new ArtilleryLoadingDoAfterEvent(args.Slot, netEnt),
            ent)
            {
                BreakOnMove = true,
                NeedHand = true,
                BreakOnHandChange = false
            };

        _doAfter.TryStartDoAfter(doAfterEvent);
    }

    private void HandleInsert (Entity<ArtilleryBreechComponent> ent, ref ArtilleryLoadingDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.IsLoading = true;

        _itemSlots.TryInsert(ent, args.TargetSlot, GetEntity(args.TargetEnt), args.User);
    }

    private void OnEjectAttempt(Entity<ArtilleryBreechComponent> ent, ref ItemSlotEjectAttemptEvent args) => args.Cancelled = ent.Comp.State != ArtilleryBreechState.Open;

    private void OnInserted(Entity<ArtilleryBreechComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_net.IsServer)
            return;

        UpdateUiFromBreech(ent);

        SpriteSpecifier? loadedSprite = null;

        if (TryComp<ArtilleryShellComponent>(args.Entity, out var shellComp))
        {
            loadedSprite = shellComp.LoadedSprite;
            _appearance.SetData(ent, ArtilleryBreechVisuals.LoadedState, $"{shellComp.Name}-{shellComp.LoadedSuffix}");
        }

        else if (TryComp<ArtilleryChargeComponent>(args.Entity, out var chargeComp))
        {
            loadedSprite = chargeComp.LoadedSprite;
            _appearance.SetData(ent, ArtilleryBreechVisuals.LoadedState, $"{chargeComp.Name}-{chargeComp.LoadedSuffix}");
        }

        if (loadedSprite is not null)
            _appearance.SetData(ent, ArtilleryBreechVisuals.LoadedSprite, loadedSprite);
        else
            _appearance.RemoveData(ent, ArtilleryBreechVisuals.LoadedSprite);

    }
    private void OnEjected(Entity<ArtilleryBreechComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_net.IsServer)
            return;

        UpdateUiFromBreech(ent);
        _appearance.RemoveData(ent, ArtilleryBreechVisuals.LoadedSprite);
        _appearance.RemoveData(ent, ArtilleryBreechVisuals.LoadedState);
    }

    private void SetLoadedData(Entity<ArtilleryBreechComponent> ent)
    {
        var state = ent.Comp.State;

        if (!_itemSlots.TryGetSlot(ent, ArtilleryBreechComponent.BreechSlotId, out var breechSlot))
            return;

        var loaded = breechSlot.ContainerSlot?.ContainedEntity;

        if (TryComp<ArtilleryShellComponent>(loaded, out var shellComp))
            {
            var loadedState = state switch
                {
                ArtilleryBreechState.Closing => shellComp.LoadingSuffix,
                ArtilleryBreechState.Opening => shellComp.UnloadingSuffix,
                _ => shellComp.LoadedSuffix
                };
            _appearance.SetData(ent, ArtilleryBreechVisuals.LoadedState, $"{shellComp.Name}-{loadedState}");
            }
        else if (TryComp<ArtilleryChargeComponent>(loaded, out var chargeComp))
            {
            var loadedState = state switch
                {
                ArtilleryBreechState.Closing => chargeComp.LoadingSuffix,
                ArtilleryBreechState.Opening => chargeComp.UnloadingSuffix,
                _ => chargeComp.LoadedSuffix
                };
            _appearance.SetData(ent, ArtilleryBreechVisuals.LoadedState, $"{chargeComp.Name}-{loadedState}");
            }
    }

    public void ToggleBreechState(Entity<ArtilleryBreechComponent> ent)
    {
        if (!_net.IsServer)
            return;

        if (ent.Comp.State is ArtilleryBreechState.Closing or ArtilleryBreechState.Opening)
            return;

        // Reset last toggled time
        ent.Comp.LastToggled = _timing.CurTime;

        var animTime = ent.Comp.State == ArtilleryBreechState.Open ? ent.Comp.ClosingAnimationTime : ent.Comp.OpeningAnimationTime;

        // Flip state
        ent.Comp.State = ent.Comp.State == ArtilleryBreechState.Open ? ArtilleryBreechState.Closing : ArtilleryBreechState.Opening;
        ent.Comp.NextStateChange = _timing.CurTime + animTime;

        Dirty(ent, ent.Comp);

        _appearance.SetData(ent, ArtilleryBreechVisuals.State, ent.Comp.State);
        SetLoadedData(ent);

        // Handle the sounds
        var sound = ent.Comp.State == ArtilleryBreechState.Opening ? ent.Comp.SoundOpen : ent.Comp.SoundClose;
        _audio.PlayPvs(sound, ent.Owner);

        UpdateUiFromBreech(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<ArtilleryBreechComponent>();
        while (query.MoveNext(out var uid, out var breech))
        {
            if (breech.NextStateChange is null || breech.NextStateChange >= _timing.CurTime)
                continue;

            breech.State = breech.State == ArtilleryBreechState.Opening ? ArtilleryBreechState.Open : ArtilleryBreechState.Closed;
            _appearance.SetData(uid, ArtilleryBreechVisuals.State, breech.State);
            SetLoadedData((uid, breech));

            breech.NextStateChange = null;
            Dirty(uid, breech);
            UpdateUiFromBreech((uid, breech));
        }
    }

#endregion
#region Firing

    public bool TryGetPartWithComp<T>(EntityUid ent, Enum partEnum, [NotNullWhen(true)] out T? partComp, [NotNullWhen(true)] out EntityUid? partUid) where T : class, IComponent
    {
        partUid = default;
        partComp = default;

        if (!TryComp<MultipartMachineComponent>(ent, out var multipartMachineComp))
            return false;

        if (!_multiPartMachineSystem.TryGetPartEntity((ent, multipartMachineComp), partEnum, out partUid))
            return false;

        if (!TryComp<T>(partUid, out partComp))
            return false;

        return true;
    }
    private bool GetBreechState(EntityUid ent, Enum breechEnum,
                                [NotNullWhen(true)] out ArtilleryBreechComponent? breechComp,
                                [NotNullWhen(true)] out EntityUid? breechUid,
                                [NotNullWhen(true)] out ItemSlot? breechSlot,
                                out EntityUid? containedUid)
    {
        breechComp = default;
        breechUid = default;
        breechSlot = default;
        containedUid = default;

        if (!TryGetPartWithComp<ArtilleryBreechComponent>(ent, breechEnum, out breechComp, out breechUid))
            return false;

        if (!_itemSlots.TryGetSlot(breechUid.Value, ArtilleryBreechComponent.BreechSlotId, out breechSlot))
            return false;

        if (breechSlot.ContainerSlot is null)
            return false;

        containedUid = breechSlot.ContainerSlot.ContainedEntity;
        return true;
    }

    private bool CanFire(Entity<ArtilleryBodyComponent> ent)
    {
        if (!TryComp<MultipartMachineComponent>(ent, out var assembled))
            return false;

        if (!assembled.IsAssembled)
            return false;

        if (!GetBreechState(ent, ArtilleryParts.ForwardBreech, out var forwardBreechComp, out _, out _, out var shellUid) ||
            !GetBreechState(ent, ArtilleryParts.RearBreech, out var rearBreechComp, out _, out _, out var chargeUid))
            return false;

        if (shellUid is null || chargeUid is null)
            return false;

        if (!HasComp<ArtilleryShellComponent>(shellUid) ||
            !TryComp<ArtilleryChargeComponent>(chargeUid, out var chargeComp))
            return false;

        if (chargeComp.Spent)
            return false;

        if (forwardBreechComp.State != ArtilleryBreechState.Closed || rearBreechComp.State != ArtilleryBreechState.Closed)
            return false;

        return true;
    }
    private void Fire(Entity<ArtilleryBodyComponent> ent, ref ArtilleryFireCall args)
    {
        if (!_net.IsServer)
            return;

        if (!CanFire(ent))
            return;

        if (!TryGetPartWithComp<ArtillerySecondaryBarrelComponent>(ent, ArtilleryParts.SecondaryBarrel, out var barrelComp, out var barrelUid))
            return;

        if (!GetBreechState(ent, ArtilleryParts.ForwardBreech, out _, out _, out var forwardBreechSlot, out var shellUid) ||
            !GetBreechState(ent, ArtilleryParts.RearBreech, out _, out var rearBreechUid, out var rearBreechSlot, out var chargeUid))
            return;

        if (!TryComp<ArtilleryShellComponent>(shellUid!.Value, out var shell) ||
            !TryComp<ArtilleryChargeComponent>(chargeUid!.Value, out var charge))
            return;

        var barrelTransform = Transform(barrelUid.Value);
        var barrelLoc = _transform.GetMapCoordinates(barrelUid.Value);
        var barrelRot = _transform.GetWorldRotation(barrelUid.Value);

        // Calculate the SpawnPoint for the shell at the end of the barrel
        var spawnPoint = new EntityCoordinates(barrelTransform.MapUid ?? barrelTransform.ParentUid, barrelLoc.Position + (barrelRot.ToWorldVec() * barrelComp.BarrelLength));

        // Muzzle flash effect
        var flash = Spawn(barrelComp.MuzzleFlash, spawnPoint);
        _transform.SetWorldRotation(flash, barrelRot);

        // Spawn and fire projectile
        var projectile = Spawn(shell.Prototype, spawnPoint);
        _audio.PlayPvs(charge.SoundGunshot, spawnPoint);
        _gun.ShootProjectile(projectile, barrelRot.ToWorldVec(), Vector2.Zero, ent);
        RaiseLocalEvent(ent, new ArtilleryFiredEvent(barrelLoc, GetNetEntity(barrelUid.Value), charge.Intensity, charge.SoundRumble!));

        // Clear breeches then reinsert an empty casing
        _container.Remove(shellUid.Value, forwardBreechSlot.ContainerSlot!);
        QueueDel(shellUid);
        _container.Remove(chargeUid.Value, rearBreechSlot.ContainerSlot!);
        QueueDel(chargeUid);
        TrySpawnInContainer(charge.Casing, rearBreechUid.Value, ArtilleryBreechComponent.BreechSlotId, out _);

        BuildUiState(ent);
    }
#endregion
#region BUI
    public void UpdateUiFromBreech (Entity<ArtilleryBreechComponent> ent)
    {
        if (!TryComp<MultipartMachinePartComponent>(ent, out var multipartMachinePart))
            return;
        if (!TryComp<ArtilleryBodyComponent>(multipartMachinePart.Master, out var masterComp))
            return;
        BuildUiState(new Entity<ArtilleryBodyComponent>(multipartMachinePart.Master.Value, masterComp));
    }

    public void BuildUiState (Entity<ArtilleryBodyComponent> ent)
    {

        if (!GetBreechState(ent, ArtilleryParts.ForwardBreech, out var forwardBreechComp, out _, out _, out var shellUid) ||
            !GetBreechState(ent, ArtilleryParts.RearBreech, out var rearBreechComp, out _, out _, out var chargeUid))
            return;

        var shellNetEnt = shellUid is not null ? GetNetEntity(shellUid) : null;
        var chargeNetEnt = chargeUid is not null ? GetNetEntity(chargeUid) : null;

        var chargeValid = TryComp<ArtilleryChargeComponent>(chargeUid, out var chargeComp) && !chargeComp.Spent;

        if (!TryComp<MultipartMachineComponent>(ent, out var assembled))
            return;

        var canFire = CanFire(ent);

        var state = new ArtilleryConsoleUiState
        {
            RearBreechOpen = rearBreechComp.State == ArtilleryBreechState.Open,
            ForwardBreechOpen = forwardBreechComp.State == ArtilleryBreechState.Open,
            ChargeValid = chargeValid,
            RearBreechContains = chargeNetEnt,
            ForwardBreechContains = shellNetEnt,

            Assembled = assembled.IsAssembled,
            CanFire = canFire,
        };
        if (!TryComp<DeviceLinkSourceComponent>(ent, out var sourceComp))
            return;
        foreach (var key in sourceComp.LinkedPorts.Keys)
        {
            if (HasComp<ArtilleryConsoleComponent>(key))
                _ui.SetUiState(key, ArtilleryConsoleUiKey.Key, state);
        }
    }
#endregion
}
