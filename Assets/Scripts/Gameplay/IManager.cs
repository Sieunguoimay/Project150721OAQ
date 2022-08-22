namespace Gameplay
{
    public interface IManager : IInjectable
    {
        void OnSetup();
        void OnCleanup();
        void OnGameStart();
        void OnGameEnd();
        void OnGameReset();
    }
}