using System;
using Gameplay.Player;
using Gameplay.Visual;
using Gameplay.Visual.Board;

namespace Gameplay.PlayTurn
{
    public class PlayTurnDataGenerator
    {
        private readonly BoardStateView _boardStateView;
        private readonly CoreGameplayVisualPresenter _presenter;

        public PlayTurnDataGenerator(CoreGameplayVisualPresenter presenter, BoardStateView boardStateView)
        {
            _presenter = presenter;
            _boardStateView = boardStateView;
        }

        public PlayTurnTeller Generate()
        {
            var playTurnTeller = new PlayTurnTeller();
            var turns = CreateTurns(_boardStateView.NumSides);
            playTurnTeller.SetTurns(turns, 0);
            return playTurnTeller;
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

            return new PlayTurnData(player, boardSide, decisionMaking, pieceBench,
                _boardStateView, turnIndex);
        }

        private BoardSideVisual GetBoardSideVisual(int sideIndex)
        {
            var board = _presenter.BoardVisual;
            return board.SideVisuals[sideIndex];
        }
    }
}