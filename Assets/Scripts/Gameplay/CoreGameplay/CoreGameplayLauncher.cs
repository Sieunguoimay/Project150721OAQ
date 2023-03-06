using System;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Entities.Stage;
using UnityEngine;

namespace Gameplay.CoreGameplay
{
    [CreateAssetMenu]
    public class CoreGameplayLauncher : BaseGenericDependencyInversionScriptableObject<CoreGameplayLauncher>
    {
        private CoreGameplayController _controller;
        private IGameplayContainer _gameplayContainer;
        private IBoardMoveSimulationResultHandler _boardSimulationResultHandler;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            _controller = new CoreGameplayController();
            binder.Bind<ICoreGameplayController>(_controller);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<ICoreGameplayController>();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _gameplayContainer = Resolver.Resolve<IGameplayContainer>();
            _boardSimulationResultHandler = Resolver.Resolve<IBoardMoveSimulationResultHandler>();
        }

        public void Load()
        {
            _controller.Install(null,
                new CoreGameplayDataAccess(_gameplayContainer.MatchData), _boardSimulationResultHandler);
        }

        public void Unload()
        {
            _controller.Uninstall();
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