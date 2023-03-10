using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;

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
                DecisionMakingData = CreateDecisionMakingData()
            };
        }

        private DecisionMakingData CreateDecisionMakingData()
        {
            var citizenTiles = _boardEntityAccess.Board.CitizenTiles;
            var turnIndex = _turnEntity.TurnIndex;
            var numTiles = citizenTiles.Length / _turnEntity.NumTurns;

            var rangeFrom = turnIndex * numTiles + turnIndex + 1;

            var options = new List<DecisionOption>();
            for (var i = 0; i < numTiles; i++)
            {
                var tileIndex = rangeFrom + i;
                if (_boardEntityAccess.TileEntities[tileIndex].PieceEntities.Count > 0)
                {
                    options.Add(new DecisionOption {TileIndex = tileIndex});
                }
            }

            return new DecisionMakingData
            {
                Options = options.ToArray(),
                TurnIndex = _turnEntity.TurnIndex
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
        public DecisionMakingData DecisionMakingData;
        public int CurrentTurnIndex;
        public int NumTurns;
    }
}