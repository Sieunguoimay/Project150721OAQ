using System.Collections.Generic;
using System.Linq;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveDecisionOptionFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;

        public MoveDecisionOptionFactory(BoardEntityAccess boardEntityAccess)
        {
            _boardEntityAccess = boardEntityAccess;
        }

        public MoveDecisionMakingData CreateDecisionMakingData(ExtractedTurnData turnData)
        {
            var numCitizenTiles = _boardEntityAccess.Board.CitizenTiles.Length;
            var turnIndex = turnData.CurrentTurnIndex;
            var numTiles = numCitizenTiles / turnData.NumTurns;

            var rangeFrom = turnIndex * numTiles + turnIndex + 1;

            var options = new List<SimpleMoveOption>();
            for (var i = 0; i < numTiles; i++)
            {
                var tileIndex = rangeFrom + i;
                if (_boardEntityAccess.TileEntities[tileIndex].PieceEntities.Count > 0)
                {
                    options.Add(new SimpleMoveOption {TileIndex = tileIndex, Direction = false});
                    options.Add(new SimpleMoveOption {TileIndex = tileIndex, Direction = true});
                }
            }
            return new MoveDecisionMakingData
            {
                Options = options.Select(o => o as MoveOption).ToArray(),
                TurnIndex = turnData.CurrentTurnIndex
            };
        }
    }


    public class MoveDecisionMakingData
    {
        public MoveOption[] Options;
        public int TurnIndex;
    }

    public class MoveOption
    {
    }

    public class SimpleMoveOption : MoveOption
    {
        public int TileIndex;
        public bool Direction;
    }
}