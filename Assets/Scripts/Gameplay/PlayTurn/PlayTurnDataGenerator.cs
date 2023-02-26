using System;

namespace Gameplay.PlayTurn
{
    public class PlayTurnDataGenerator : BaseDependencyInversionScriptableObject
    {
        private GameState.GameState _gameState;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
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
                
            }
            else if(gameState.CurrentState == GameState.GameState.State.InMenu)
            {
                
            }
        }
    }
}