using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.CoreGameplay.Presenters;

namespace Gameplay.CoreGameplay.Controllers
{
    public class CoreGameplayInstallController
    {
        public CoreGameplayContainer Container { get; } = new();
        
        private CoreGameplayInstaller _installer;
        public void Install(IInteractResultPresenter resultPresenter, ICoreGameplayDataAccess dataAccess)
        {
            _installer = new CoreGameplayInstaller(Container);

            _installer.InstallEntities(dataAccess);
            _installer.InstallRefreshRequest(resultPresenter);
            _installer.InstallPiecesInteract(resultPresenter);
            _installer.InstallBoardMoveSimulation(resultPresenter);
        }

        public void Uninstall()
        {
            _installer.Uninstall();
        }
    }

    public class CoreGameplayContainer
    {
        public IRefreshRequester RefreshRequester;
        public IPiecesInteractor PiecesInteractor;
        public IBoardMoveSimulator BoardMoveSimulator;
    }
}