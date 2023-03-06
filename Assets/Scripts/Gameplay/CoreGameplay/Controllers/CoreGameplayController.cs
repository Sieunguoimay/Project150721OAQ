using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Controllers
{
    public interface ICoreGameplayController
    {
        void RequestRefresh(IRefreshResultHandler resultPresenter);
        void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData);
        void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData);
        void RunSimulation(MoveSimulationInputData inputData);
    }

    public class CoreGameplayController : ICoreGameplayController
    {
        public CoreGameplayContainer Container { get; } = new();

        private CoreGameplayInstaller _installer;

        public void Install(IPieceInteractResultHandler resultPresenter, ICoreGameplayDataAccess dataAccess, IBoardMoveSimulationResultHandler simulationResultHandler)
        {
            _installer = new CoreGameplayInstaller(Container);

            _installer.InstallEntities(dataAccess);
            _installer.InstallRefreshRequest();
            _installer.InstallPiecesInteract(resultPresenter);
            _installer.InstallBoardMoveSimulation(simulationResultHandler);
        }

        public void Uninstall()
        {
            _installer.Uninstall();
        }

        public void RequestRefresh(IRefreshResultHandler resultPresenter)
        {
            Container.RefreshRequester.Refresh(resultPresenter);
        }

        public void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData)
        {
            Container.PiecesInteractor.MovePieceToNewTile(moveData);
        }

        public void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData)
        {
            Container.PiecesInteractor.MovePiecesToPocket(moveData);
        }

        public void RunSimulation(MoveSimulationInputData inputData)
        {
            Container.BoardMoveSimulator.RunSimulation(inputData);
        }
    }


    public class CoreGameplayContainer
    {
        public IRefreshRequester RefreshRequester;
        public IPiecesInteractor PiecesInteractor;
        public IBoardMoveSimulator BoardMoveSimulator;
    }
}