using Content.Shared.Artillery.Components;
using Robust.Client.GameObjects;
using Content.Shared.Artillery;
using Robust.Shared.Utility;

namespace Content.Client.Artillery;

public sealed class ArtilleryBreechVisualizerSystem : VisualizerSystem<ArtilleryBreechComponent>
{
    protected override void OnAppearanceChange(EntityUid ent, ArtilleryBreechComponent comp, ref AppearanceChangeEvent args)
    {
        AppearanceSystem.TryGetData<ArtilleryBreechState>(ent, ArtilleryBreechVisuals.State, out var state, args.Component);
        AppearanceSystem.TryGetData<string>(ent, ArtilleryBreechVisuals.LoadedState, out var loadedState, args.Component);
        AppearanceSystem.TryGetData<SpriteSpecifier?>(ent, ArtilleryBreechVisuals.LoadedSprite, out var loadedSprite, args.Component);

        // Set breech state, Open/Closed
        var openLayer = SpriteSystem.LayerMapGet((ent, args.Sprite), ArtilleryBreechVisuals.OpenLayer);
        var breechString = state switch
        {
            ArtilleryBreechState.Open => "breech-open",
            ArtilleryBreechState.Closing => "breech-closing",
            ArtilleryBreechState.Opening => "breech-opening",
            _ => "breech-closed"

        };
        SpriteSystem.LayerSetRsiState((ent, args.Sprite), openLayer, breechString);

        // Handle showing the loaded object
        var loadedLayer = SpriteSystem.LayerMapGet((ent, args.Sprite), ArtilleryBreechVisuals.LoadedLayer);

        if (loadedSprite is null || loadedState is null)
            SpriteSystem.LayerSetVisible((ent, args.Sprite), loadedLayer, false);
        else
        {
            SpriteSystem.LayerSetVisible((ent, args.Sprite), loadedLayer, state != ArtilleryBreechState.Closed);
            SpriteSystem.LayerSetSprite((ent, args.Sprite), loadedLayer, loadedSprite);
            SpriteSystem.LayerSetRsiState((ent, args.Sprite), loadedLayer, loadedState);
        }
    }
}
