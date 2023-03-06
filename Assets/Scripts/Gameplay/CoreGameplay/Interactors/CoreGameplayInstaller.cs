using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInstaller
    {
        private BoardEntity _board;
        private readonly CoreGameplayContainer _container;
        private PiecesInteractor.InnerPiecesInteractor _innerPiecesInteractor;

        public CoreGameplayInstaller(CoreGameplayContainer container)
        {
            _container = container;
        }

        public void InstallEntities(ICoreGameplayDataAccess dataAccess)
        {
            var boardData = dataAccess.GetBoardData();
            _board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _innerPiecesInteractor = new PiecesInteractor.InnerPiecesInteractor(_board);
        }

        public void InstallRefreshRequest()
        {
            var refreshRequester = new RefreshRequester(_board);
            _container.RefreshRequester = refreshRequester;
        }

        public void InstallPiecesInteract(IPieceInteractResultHandler interactResultHandler)
        {
            _container.PiecesInteractor = new PiecesInteractor(_innerPiecesInteractor,interactResultHandler);
        }

        public void InstallBoardMoveSimulation(IBoardMoveSimulationResultHandler simulationResultHandler)
        {
            var simulator = new BoardMoveSimulator(_board, simulationResultHandler, _innerPiecesInteractor);
            _container.BoardMoveSimulator = simulator;
        }

        public void Uninstall()
        {
            _container.RefreshRequester = null;
            _container.PiecesInteractor = null;
            _container.BoardMoveSimulator = null;
        }
    }
}