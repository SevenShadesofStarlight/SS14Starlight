using Content.Shared.Artillery.Components;
using Content.Shared._Starlight.Camera;
using Robust.Shared.Audio.Systems;
using Content.Shared.Artillery;
using Content.Shared.Camera;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server.Artillery;

public sealed partial class ServerArtillerySystem : EntitySystem
{
    #pragma warning disable RA0051
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ScreenshakeSystem _shake = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtilleryBodyComponent, ArtilleryFiredEvent>(OnFired);
    }

    private void OnFired(Entity<ArtilleryBodyComponent> ent, ref ArtilleryFiredEvent args)
    {
        var resolvedSound = _audio.ResolveSound(args.SoundRumble);
        var entCentre = new EntityCoordinates(GetEntity(args.BarrelEnt), args.Centre.Position);
        var players = Filter.Empty().AddInRange(args.Centre, args.Intensity, _playerManager, EntityManager);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid uid)
                continue;

            var playerPos = _transform.GetWorldPosition(player.AttachedEntity!.Value);
            var delta = args.Centre.Position - playerPos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(0.01f, 0);

            var distance = delta.Length();
            var effect = 5 * MathF.Pow(args.Intensity, 0.5f) * (1 - (distance / args.Intensity));
            if (effect > 0.01f)
            {
                _recoil.KickCamera(uid, -delta.Normalized() * effect * 0.4f);
            }
        }
        _shake.Screenshake(players, new ScreenshakeParameters { Trauma = 0.6f, DecayRate = 0.05f, Frequency = 0.014f }, null);
        _audio.PlayGlobal(resolvedSound, players, true);
    }
}
