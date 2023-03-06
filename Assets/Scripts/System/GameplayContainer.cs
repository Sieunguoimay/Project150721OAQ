using Gameplay.Entities.Stage;
using Gameplay.PlayTurn;
using Gameplay.Visual.Board;

namespace System
{
    public interface IGameplayContainer
    {
        IPlayTurnTeller PlayTurnTeller { get; }
        MatchData MatchData { get; }
    }

    public class GameplayContainer : IGameplayContainer
    {
        public Board Board { get; private set; }
        public IPlayTurnTeller PlayTurnTeller { get; private set; }
        public MatchData MatchData { get; private set; }

        public void PublicBoard(Board board)
        {
            Board = board;
        }

        public void PublicPlayTurnTeller(IPlayTurnTeller playTurnTeller)
        {
            PlayTurnTeller = playTurnTeller;
        }

        public void PublicMatchData(MatchData matchData)
        {
            MatchData = matchData;
        }

        public void Cleanup()
        {
            Board = null;
            PlayTurnTeller = null;
            MatchData = null;
        }
    }
}