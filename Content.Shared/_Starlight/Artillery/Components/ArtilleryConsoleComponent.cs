using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Artillery.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArtilleryConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public NetEntity? ArtilleryBody;

    [DataField]
    public ProtoId<SinkPortPrototype> LinkingPort = "ArtilleryDataReceiver";
}
