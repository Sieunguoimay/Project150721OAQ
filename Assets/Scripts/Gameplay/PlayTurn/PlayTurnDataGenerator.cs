using System;
using Framework.Resolver;
using Gameplay.Board;
using Gameplay.Entities.Stage;
using Gameplay.Player;

namespace Gameplay.PlayTurn
{
    public class PlayTurnDataGenerator : BaseDependencyInversionScriptableObject
    {
        private GameState.GameState _gameState;
        private MatchData _matchData;
        private IPlayTurnData[] _turns;
        private readonly PlayTurnTeller _playTurnTeller = new();

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            binder.Bind<IPlayTurnTeller>(_playTurnTeller);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
            _matchData = Resolver.Resolve<MatchData>();
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState.GameState gameState)
        {
            if (gameState.CurrentState == GameState.GameState.State.Playing)
            {
                CreateTurns();
                _playTurnTeller.SetTurns(_turns, 0);
            }
            else if (gameState.CurrentState == GameState.GameState.State.InMenu)
            {
                CleanupTurns();
            }
        }

        private void CreateTurns()
        {
            _turns = new IPlayTurnData[_matchData.playerNum];

            _turns[0] = CreatePlayTurnDataForRealPlayer(0);

            for (var i = 0; i < _matchData.playerNum; i++)
            {
                _turns[i] = CreatePlayTurnDataForFakePlayer(i);
            }
        }

        private void CleanupTurns()
        {
            _turns = null;
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
            var board = Resolver.Resolve<BoardManager>().Board;
            var boardSide = board.Sides[turnIndex];

            var player = playerFactory.CreatePlayer();
            var decisionMaking = playerFactory.CreatePlayerDecisionMaking(boardSide);
            var pieceBench = playerFactory.CreatePieceBench(boardSide);
            
            return new PlayTurnData(player, boardSide, decisionMaking, pieceBench);
        }
    }
}