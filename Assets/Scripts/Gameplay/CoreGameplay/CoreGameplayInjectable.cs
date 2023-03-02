using System;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.Entities.Stage;
using Gameplay.GameState;

namespace Gameplay.CoreGameplay
{
    public class CoreGameplayInjectable : BaseDependencyInversionScriptableObject
    {
        private CoreGameplayInstallController _installController;
        private IGameState _gameState;
        private GameplayContainer _gameplayContainer;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            _installController = new CoreGameplayInstallController();
            binder.Bind<CoreGameplayContainer>(_installController.Container);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<CoreGameplayContainer>();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _gameState = Resolver.Resolve<IGameState>();
            _gameplayContainer = Resolver.Resolve<GameplayContainer>();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState.GameState obj)
        {
            if (_gameState.CurrentState == GameState.GameState.State.Playing)
            {
                _installController.Install(null, new CoreGameplayDataAccess(_gameplayContainer.MatchData));
            }
            else
            {
                _installController.Uninstall();
            }
        }

        private class CoreGameplayDataAccess : ICoreGameplayDataAccess
        {
            private readonly MatchData _matchData;

            public CoreGameplayDataAccess(MatchData matchData)
            {
                _matchData = matchData;
            }

            public BoardData GetBoardData()
            {
                return new()
                {
                    NumSides = _matchData.playerNum,
                    TilesPerSide = _matchData.tilesPerGroup,
                    PiecesPerTile = _matchData.numCitizensInTile
                };
            }
        }
    }
}