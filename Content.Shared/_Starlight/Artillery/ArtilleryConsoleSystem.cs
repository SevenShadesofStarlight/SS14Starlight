using Content.Shared.Artillery.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.DeviceLinking;
using Content.Shared.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Shared.Artillery;
public sealed partial class ArtilleryConsoleSystem : EntitySystem
{
    #pragma warning disable RA0051
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ArtillerySystem _artillery = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _signal = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    #pragma warning restore

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtilleryConsoleComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<ArtilleryConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<ArtilleryConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<ArtilleryConsoleComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<ArtilleryConsoleComponent, BeforeActivatableUIOpenEvent>(OnConsoleBeforeUiOpened);

        SubscribeLocalEvent<ArtilleryConsoleComponent, ArtilleryToggleForwardBreechMessage>(ToggleForwardBreech);
        SubscribeLocalEvent<ArtilleryConsoleComponent, ArtilleryToggleRearBreechMessage>(ToggleRearBreech);
        SubscribeLocalEvent<ArtilleryConsoleComponent, ArtilleryFireMessage>(CallFire);
    }

    private void OnMapInit(EntityUid uid, ArtilleryConsoleComponent comp, ref MapInitEvent args)
    {
        if (!_entityManager.TryGetComponent<DeviceLinkSinkComponent>(uid, out var sink))
            return;

        foreach (var source in sink.LinkedSources)
        {
            if (!HasComp<ArtilleryBodyComponent>(source))
                continue;

            comp.ArtilleryBody = GetNetEntity(source);
            Dirty(uid, comp);
            return;
        }
    }
    private void OnConsoleBeforeUiOpened(Entity<ArtilleryConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        var bodyEnt = GetEntity(ent.Comp.ArtilleryBody);
        if (bodyEnt is null)
            return;

        if (!TryComp<ArtilleryBodyComponent>(bodyEnt, out var bodyComp))
            return;

        _artillery.BuildUiState(new Entity<ArtilleryBodyComponent>(bodyEnt.Value, bodyComp));
    }
    private void OnNewLink(EntityUid uid, ArtilleryConsoleComponent comp, ref NewLinkEvent args)
    {
        if (!HasComp<ArtilleryBodyComponent>(args.Source))
            return;

        comp.ArtilleryBody = GetNetEntity(args.Source);
        Dirty(uid, comp);
    }

    private void OnPortDisconnected(EntityUid uid, ArtilleryConsoleComponent comp, ref PortDisconnectedEvent args)
    {
        if (args.Port != comp.LinkingPort)
            return;

        comp.ArtilleryBody = null;
        Dirty(uid, comp);
    }

    private void OnAnchorChanged(EntityUid uid, ArtilleryConsoleComponent comp, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        CheckRange(uid, comp);
    }

    private void CheckRange(EntityUid uid, ArtilleryConsoleComponent comp)
    {
        if (!TryComp<DeviceLinkSinkComponent>(uid, out var sink) || sink.LinkedSources.Count < 1)
            return;

        if (!TryGetEntity(comp.ArtilleryBody, out var uidBody))
            return;

        if (!TryComp<DeviceLinkSourceComponent>(uidBody, out var source))
            return;

        var xformMonitor = Transform(uid);
        var xformReactor = Transform(uidBody.Value);
        var posMonitor = _transform.GetWorldPosition(xformMonitor);
        var posReactor = _transform.GetWorldPosition(xformReactor);

        if (xformMonitor.MapID == xformReactor.MapID && (posMonitor - posReactor).Length() <= source.Range)
            return;

        _ui.CloseUi(uid, ArtilleryConsoleUiKey.Key);
        comp.ArtilleryBody = null;
        _signal.RemoveSinkFromSource(uidBody.Value, uid, source, sink);
        Dirty(uid, comp);
    }

    private void ToggleForwardBreech(Entity<ArtilleryConsoleComponent> ent, ref ArtilleryToggleForwardBreechMessage args)
    {
        var bodyEnt = GetEntity(ent.Comp.ArtilleryBody);
        if (bodyEnt is null)
            return;

        if (!_artillery.TryGetPartWithComp<ArtilleryBreechComponent>(bodyEnt.Value, ArtilleryParts.ForwardBreech, out var breechComp, out var breechUid))
            return;

        if (breechComp.LastToggled+breechComp.ToggleCooldown > _timing.CurTime)
            return;

        _artillery.ToggleBreechState(new Entity<ArtilleryBreechComponent>(breechUid.Value, breechComp));
    }
    private void ToggleRearBreech(Entity<ArtilleryConsoleComponent> ent, ref ArtilleryToggleRearBreechMessage args)
    {
        var bodyEnt = GetEntity(ent.Comp.ArtilleryBody);
        if (bodyEnt is null)
            return;

        if (!_artillery.TryGetPartWithComp<ArtilleryBreechComponent>(bodyEnt.Value, ArtilleryParts.RearBreech, out var breechComp, out var breechUid))
            return;

        if (breechComp.LastToggled+breechComp.ToggleCooldown > _timing.CurTime)
            return;

        _artillery.ToggleBreechState(new Entity<ArtilleryBreechComponent>(breechUid.Value, breechComp));
    }
    private void CallFire(Entity<ArtilleryConsoleComponent> ent, ref ArtilleryFireMessage args)
    {
        var bodyEnt = GetEntity(ent.Comp.ArtilleryBody);
        if (bodyEnt is null)
            return;
        RaiseLocalEvent(bodyEnt.Value, new ArtilleryFireCall());
    }
}
