using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Presenters;

namespace Gameplay.CoreGameplay.Controllers
{
    public class CoreGameplayInstallController
    {
        public CoreGameplayContainer Container { get; private set; }
        public RefreshResultPresenter RefreshResultPresenter { get; private set; }

        public void Install()
        {
            Container = new CoreGameplayContainer();

            var installer = new CoreGameplayInstaller(Container);
            var dataAccess = new BoardConfigDatabase();
            RefreshResultPresenter = new RefreshResultPresenter(Container);

            installer.InstallEntities(dataAccess);
            installer.InstallRefreshRequest(RefreshResultPresenter);
            installer.InstallPiecesInteract(RefreshResultPresenter);
            installer.InstallBoardMoveSimulation(RefreshResultPresenter);
        }

        public void Uninstall()
        {
            Container.RefreshRequester = null;
            Container = null;
        }

        private class BoardConfigDatabase : IEntitiesDataAccess
        {
            public BoardData GetBoardData()
            {
                return new()
                {
                    NumSides = 2,
                    TilesPerSide = 5,
                    PiecesPerTile = 5
                };
            }
        }
    }

    public class CoreGameplayContainer
    {
        public IRefreshRequester RefreshRequester;
        public IPiecesInteractor PiecesInteractor;
        public IBoardMoveSimulator BoardMoveSimulator;
    }
}