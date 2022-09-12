namespace System
{
    public sealed class GameFlowManager
    {
        private readonly IGameFlowHandler _handler;
        public GameState CurrentState { get; private set; }
        public event Action StateChanged;

        public GameFlowManager(IGameFlowHandler handler)
        {
            _handler = handler;
            CurrentState = GameState.InMenu;
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            StateChanged?.Invoke();
        }

        public void StartGame()
        {
            _handler.StartGame();
            ChangeState(GameState.DuringGameplay);
        }

        public void OnReplayMatch()
        {
            _handler.ReplayMatch();
            ChangeState(GameState.BeforeGameplay);
        }

        public void OnResetGame()
        {
            _handler.ResetGame();
            ChangeState(GameState.InMenu);
        }

        public enum GameState
        {
            InMenu,
            BeforeGameplay,
            DuringGameplay,
            AfterGameplay,
        }
    }

    public interface IGameFlowHandler
    {
        void StartGame();
        void ResetGame();
        void ReplayMatch();
    }
}