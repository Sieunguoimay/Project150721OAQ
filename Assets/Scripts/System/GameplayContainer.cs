using Gameplay.PlayTurn;
using Gameplay.Visual.Board;

namespace System
{
    public interface IGameplayContainer
    {
        Board Board { get; }
        IPlayTurnTeller PlayTurnTeller { get; }
    }

    public class GameplayContainer : IGameplayContainer
    {
        public Board Board { get; private set; }
        public IPlayTurnTeller PlayTurnTeller { get; private set; }

        public void PublicBoard(Board board)
        {
            Board = board;
        }

        public void PublicPlayTurnTeller(IPlayTurnTeller playTurnTeller)
        {
            PlayTurnTeller = playTurnTeller;
        }
        public void Cleanup()
        {
            Board = null;
            PlayTurnTeller = null;
        }
    }
}