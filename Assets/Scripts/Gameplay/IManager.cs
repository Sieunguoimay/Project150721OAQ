namespace Gameplay
{
    public interface IManager
    {
        void OnInitialize();
        void OnSetup();
        void OnCleanup();
        void OnGameStart();
        void OnGameEnd();
        void OnGameReset();
    }
}