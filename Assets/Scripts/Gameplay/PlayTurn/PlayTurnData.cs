using Gameplay.Board;
using Gameplay.DecisionMaking;
using Gameplay.Player;

namespace Gameplay.PlayTurn
{
    public interface IPlayTurnData
    {
        Player.Player Player { get; }
        BoardSide BoardSide { get; }
        IPlayerDecisionMaking DecisionMaking { get; }
        PieceBench PieceBenche { get; }
    }

    public class PlayTurnData : IPlayTurnData
    {
        public PlayTurnData(Player.Player player, BoardSide boardSide, IPlayerDecisionMaking decisionMaking, PieceBench pieceBenche)
        {
            Player = player;
            BoardSide = boardSide;
            DecisionMaking = decisionMaking;
            PieceBenche = pieceBenche;
        }

        public Player.Player Player { get; }
        public BoardSide BoardSide { get; }
        public IPlayerDecisionMaking DecisionMaking { get; }
        public PieceBench PieceBenche { get; }
    }
}