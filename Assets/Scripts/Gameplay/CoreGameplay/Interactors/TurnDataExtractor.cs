using System.Collections.Generic;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public class TurnDataExtractor : SelfBindingDependencyInversionUnit
    {
        private BoardEntityAccess _boardEntityAccess;

        private TurnEntity _turnEntity;
        public ExtractedTurnData ExtractedTurnData { get; private set; }

        public void SetTurnEntity(TurnEntity turnEntity)
        {
            _turnEntity = turnEntity;
            ExtractedTurnData = ExtractTurnData();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
        }

        private IReadOnlyList<TileEntity> GetCitizenTileEntitiesByTurn(int turn)
        {
            var numTiles = _boardEntityAccess.TileEntities.Length / _turnEntity.NumTurns;
            var rangeFrom = turn * numTiles + 1;
            var rangeTo = (turn + 1) * numTiles;
            return _boardEntityAccess.TileEntities[rangeFrom..rangeTo];
        }

        private ExtractedTurnData ExtractTurnData()
        {
            return new()
            {
                NumTurns = _turnEntity.NumTurns,
                CurrentTurnIndex = _turnEntity.TurnIndex,
                CitizenTileEntitiesOfCurrentTurn = GetCitizenTileEntitiesByTurn(_turnEntity.TurnIndex),
                PocketEntity = _boardEntityAccess.GetPocketAtIndex(_turnEntity.TurnIndex)
            };
        }

        public void NextTurn()
        {
            _turnEntity.TurnIndex = (_turnEntity.TurnIndex + 1) % _turnEntity.NumTurns;
            ExtractedTurnData = ExtractTurnData();
        }
    }

    public class ExtractedTurnData
    {
        public IReadOnlyList<TileEntity> CitizenTileEntitiesOfCurrentTurn;
        public PocketEntity PocketEntity;

        public int CurrentTurnIndex;
        public int NumTurns;
    }
}