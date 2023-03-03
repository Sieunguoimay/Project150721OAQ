using System;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.Entities.Stage;
using Gameplay.Visual;

namespace Gameplay.CoreGameplay
{
    public class CoreGameplayInjectable : BaseDependencyInversionScriptableObject, IGameplayLoadingUnit
    {
        private CoreGameplayInstallController _installController;
        private GameplayContainer _gameplayContainer;
        private CoreGameplayVisualPresenter _presenter;
        private IGameplayLoadingHost _loadingHost;

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
            _loadingHost = Resolver.Resolve<IGameplayLoadingHost>();
            _gameplayContainer = Resolver.Resolve<GameplayContainer>();
            _presenter = Resolver.Resolve<CoreGameplayVisualPresenter>();
            // _gameState.StateChangedEvent -= OnGameStateChanged;
            // _gameState.StateChangedEvent += OnGameStateChanged;
            _loadingHost.Register(this);
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
        }

        public void Load(GameplayLoadingHost host)
        {
            _installController.Install(_presenter, new CoreGameplayDataAccess(_gameplayContainer.MatchData));
            _loadingHost.NotifyUnitLoadingDone(this);
        }

        public void Unload(GameplayLoadingHost host)
        {
            _installController.Uninstall();
            _loadingHost.NotifyUnitUnloadingDone(this);
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