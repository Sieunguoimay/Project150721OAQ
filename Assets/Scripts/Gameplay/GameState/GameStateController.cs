using System;
using Framework.Resolver;
using Framework.Services.Data;
using Gameplay.Entities.Stage.StageSelector;
using UnityEngine;

namespace Gameplay.GameState
{
    [CreateAssetMenu(menuName = "Controller/GameStateController")]
    public class GameStateController : BaseGenericDependencyInversionScriptableObject<GameStateController>
    {
        [SerializeField, DataAssetIdSelector(typeof(IStageSelectorData))]
        private string stageSelectorId;

        private IStageSelector _stageSelector;
        private IGameplayController _gameplayController;
        [field: System.NonSerialized] public GameState GameState { get; private set; }
        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            InitializeGameState();
            binder.Bind<IGameState>(GameState);
        }

        private void InitializeGameState()
        {
            GameState = new GameState();
            GameState.SetState(GameState.State.InMenu);
        }
        
        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _stageSelector = Resolver.Resolve<IStageSelector>(stageSelectorId);

            _gameplayController = Resolver.Resolve<IGameplayController>();
            _gameplayController.GameEndedEvent -= OnGameplayEnded;
            _gameplayController.GameEndedEvent += OnGameplayEnded;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameplayController.GameEndedEvent -= OnGameplayEnded;
        }

        private void OnGameplayEnded(IGameplayController obj)
        {
            GameState.SetState(GameState.State.Ended);
        }

        public void TryEnterPlayingState()
        {
            if (_stageSelector.SelectedStage != null)
            {
                GameState.SetState(GameState.State.Playing);
            }
        }

        public void ForceQuitToMenuState()
        {
            GameState.SetState(GameState.State.InMenu);
        }
    }
}