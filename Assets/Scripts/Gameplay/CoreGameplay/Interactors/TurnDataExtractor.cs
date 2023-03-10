using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Interactors.Driver;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors
{
    public class TurnDataExtractor
    {
        private readonly BoardEntityAccess _boardEntityAccess;
        private readonly TurnEntity _turnEntity;

        public event Action<TurnDataExtractor> TurnChangedEvent;

        public TurnDataExtractor(BoardEntityAccess boardEntityAccess, TurnEntity turnEntity)
        {
            _boardEntityAccess = boardEntityAccess;
            _turnEntity = turnEntity;
        }

        private IReadOnlyList<TileEntity> GetTileEntitiesByTurn(int turn)
        {
            var numTiles = _boardEntityAccess.TileEntities.Length / _turnEntity.NumTurns;
            var rangeFrom = turn * numTiles;
            var rangeTo = (turn + 1) * numTiles;
            return _boardEntityAccess.TileEntities[rangeFrom..rangeTo];
        }

        public ExtractedTurnData ExtractTurnData()
        {
            return new()
            {
                NumTurns = _turnEntity.NumTurns,
                CurrentTurnIndex = _turnEntity.TurnIndex,
                TileEntitiesOfCurrentTurn = GetTileEntitiesByTurn(_turnEntity.TurnIndex),
                PocketEntity = _boardEntityAccess.GetPocketAtIndex(_turnEntity.TurnIndex),
                // MoveDecisionMakingData = CreateDecisionMakingData(_boardEntityAccess.Board.CitizenTiles.Length)
            };
        }

        public void NextTurn()
        {
            _turnEntity.TurnIndex = (_turnEntity.TurnIndex + 1) % _turnEntity.NumTurns;
            TurnChangedEvent?.Invoke(this);
        }
    }

    public class ExtractedTurnData
    {
        public IReadOnlyList<TileEntity> TileEntitiesOfCurrentTurn;
        public PocketEntity PocketEntity;
        // public MoveDecisionMakingData MoveDecisionMakingData;
        public int CurrentTurnIndex;
        public int NumTurns;
    }
}