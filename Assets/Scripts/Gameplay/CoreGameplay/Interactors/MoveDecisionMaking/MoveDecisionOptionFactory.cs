using System.Collections.Generic;

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

            var options = new List<MoveDecisionOption>();
            for (var i = 0; i < numTiles; i++)
            {
                var tileIndex = rangeFrom + i;
                if (_boardEntityAccess.TileEntities[tileIndex].PieceEntities.Count > 0)
                {
                    options.Add(new MoveDecisionOption {TileIndex = tileIndex});
                }
            }

            return new MoveDecisionMakingData
            {
                Options = options.ToArray(),
                TurnIndex = turnData.CurrentTurnIndex
            };
        }
    }

    public class MoveDecisionOption
    {
        public int TileIndex;
    }

    public class MoveDecisionMakingData
    {
        public MoveDecisionOption[] Options;
        public int TurnIndex;
    }
}