using Framework.DependencyInversion;
using Gameplay.GameState;

namespace System
{
    public class GameStateEventsHandler : DependencyInversionUnit
    {
        private GameplayEventsHandler _gameplayEventsHandler;
        private IGameState _gameState;
        private GameplayLauncher _gameplayLauncher;
        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _gameState = Resolver.Resolve<IGameState>();
            _gameplayLauncher = Resolver.Resolve<GameplayLauncher>();
            
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
        }
        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
        }
        
        private void OnGameStateChanged(GameState gameState)
        {
            switch (gameState.CurrentState)
            {
                case GameState.State.Playing:
                    _gameplayLauncher.StartGame();
                    break;
                case GameState.State.InMenu:
                    _gameplayLauncher.ClearGame();
                    break;
                case GameState.State.Paused:
                    break;
                case GameState.State.Ended:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}