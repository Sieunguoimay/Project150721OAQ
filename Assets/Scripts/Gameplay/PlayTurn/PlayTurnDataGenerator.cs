using System;
using Gameplay.Player;
using Gameplay.Visual;
using Gameplay.Visual.Board;

namespace Gameplay.PlayTurn
{
    public class PlayTurnDataGenerator
    {
        private BoardVisual _boardVisual;

        public PlayTurnTeller PlayTurnTeller { get; } = new();

        public void Generate(int numSides, BoardVisual boardVisual)
        {
            _boardVisual = boardVisual;
            var turns = CreateTurns(numSides);
            PlayTurnTeller.SetTurns(turns, 0);
        }

        private PlayTurnData[] CreateTurns(int turnNum)
        {
            var turns = new PlayTurnData[turnNum];

            turns[0] = CreatePlayTurnDataForRealPlayer(0);

            for (var i = 1; i < turnNum; i++)
            {
                turns[i] = CreatePlayTurnDataForFakePlayer(i);
            }

            return turns;
        }

        private PlayTurnData CreatePlayTurnDataForRealPlayer(int turnIndex)
        {
            return CreatePlayTurnData(turnIndex, new RealPlayerFactory());
        }

        private PlayTurnData CreatePlayTurnDataForFakePlayer(int turnIndex)
        {
            return CreatePlayTurnData(turnIndex, new FakePlayerFactory());
        }

        private PlayTurnData CreatePlayTurnData(int turnIndex, IPlayerFactory playerFactory)
        {
            var boardSide = GetBoardSideVisual(turnIndex);
            var player = playerFactory.CreatePlayer();
            var decisionMaking = playerFactory.CreatePlayerDecisionMaking(boardSide);
            var pieceBench = playerFactory.CreatePieceBench(boardSide);

            return new PlayTurnData(player, boardSide, decisionMaking, pieceBench, turnIndex);
        }

        private BoardSideVisual GetBoardSideVisual(int sideIndex)
        {
            return _boardVisual.SideVisuals[sideIndex];
        }
    }
}