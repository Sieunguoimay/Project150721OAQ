using Gameplay.Board;
using Gameplay.DecisionMaking;
using Gameplay.Player;

namespace Gameplay.PlayTurn
{
    public class PlayTurnData
    {
        public PlayTurnData(IPlayer player, BoardSide boardSide, IPlayerDecisionMaking decisionMaking, PieceBench pieceBench)
        {
            Player = player;
            BoardSide = boardSide;
            DecisionMaking = decisionMaking;
            PieceBench = pieceBench;
        }

        public IPlayer Player { get; }
        public BoardSide BoardSide { get; }
        public IPlayerDecisionMaking DecisionMaking { get; }
        public PieceBench PieceBench { get; }
    }
}