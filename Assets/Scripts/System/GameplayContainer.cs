using Gameplay.Entities.Stage;
using Gameplay.PlayTurn;
using Gameplay.Visual.Views;

namespace System
{
    public interface IGameplayContainer
    {
        IPlayTurnTeller PlayTurnTeller { get; }
        MatchData MatchData { get; }
        PiecesMovingRunner MovingRunner { get; }
    }

    public class GameplayContainer : IGameplayContainer
    {
        public IPlayTurnTeller PlayTurnTeller { get; private set; }
        public MatchData MatchData { get; private set; }
        public PiecesMovingRunner MovingRunner { get; private set; }

        public void PublicPlayTurnTeller(IPlayTurnTeller playTurnTeller)
        {
            PlayTurnTeller = playTurnTeller;
        }

        public void PublicMatchData(MatchData matchData)
        {
            MatchData = matchData;
        }

        public void PublicMovingRunner(PiecesMovingRunner movingRunner)
        {
            MovingRunner = movingRunner;
        }

        public void Cleanup()
        {
            PlayTurnTeller = null;
            MatchData = null;
            MovingRunner = null;
        }
    }
}