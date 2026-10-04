using JetBrains.Annotations;
using Robust.Shared.Map;
using Content.Server.Tabletop;

namespace Content.Server._Starlight.Tabletop
{
    [UsedImplicitly]
    public sealed partial class TabletopCribbageSetup : TabletopSetup
    {

        public override void SetupTabletop(TabletopSession session, IEntityManager entityManager)
        {
            var cribbageboard = entityManager.SpawnEntity(BoardPrototype, session.Position.Offset(0, 0));

            session.Entities.Add(cribbageboard);

            SpawnPieces(session, entityManager, session.Position.Offset(-3.1f, -10.4f));
        }

        private void SpawnPieces(TabletopSession session, IEntityManager entityManager, MapCoordinates topLeft, float separation = 0.4f)
        {
            var (mapId, x, y) = topLeft;

            const string PegsRow = "rwb";

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    switch (PegsRow[j])
                    {
                        case 'r':
                            EntityUid tempQualifier = entityManager.SpawnEntity("RedPeg", new MapCoordinates(x + (j * (separation * 1.5f)), y + (i * separation), mapId));
                            session.Entities.Add(tempQualifier);
                            break;
                        case 'w':
                            EntityUid tempQualifier1 = entityManager.SpawnEntity("WhitePeg", new MapCoordinates(x + (j * (separation * 1.5f)), y + (i * separation), mapId));
                            session.Entities.Add(tempQualifier1);
                            break;
                        case 'b':
                            EntityUid tempQualifier2 = entityManager.SpawnEntity("BluePeg", new MapCoordinates(x + (j * (separation * 1.5f)), y + (i * separation), mapId));
                            session.Entities.Add(tempQualifier2);
                            break;
                    }
                }
            }
        }
    }
}
