using Gameplay.Board;
using Gameplay.DecisionMaking;

namespace Gameplay.PlayTurn
{
    public interface IPlayTurnData
    {
         Player.Player Player { get; }
         BoardSide BoardSide { get; }
         IPlayerDecisionMaking DecisionMaking { get; }
    }
    
    public class PlayTurnData:IPlayTurnData
    {
        public PlayTurnData(Player.Player player, BoardSide boardSide, IPlayerDecisionMaking decisionMaking)
        {
            Player = player;
            BoardSide = boardSide;
            DecisionMaking = decisionMaking;
        }

        public Player.Player Player { get; }
        public BoardSide BoardSide { get; }
        public IPlayerDecisionMaking DecisionMaking { get; }
    }
}