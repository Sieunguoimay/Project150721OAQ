using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInstaller
    {
        private BoardEntity _board;
        private readonly ICoreGameplayDataAccess _dataAccess;
        private PiecesInteractor.InnerPiecesInteractor _innerPiecesInteractor;
        private readonly IPieceInteractResultHandler _interactResultHandler;
        private readonly IBoardMoveSimulationResultHandler _simulationResultHandler;

        public CoreGameplayInstaller(ICoreGameplayDataAccess dataAccess, 
            IPieceInteractResultHandler interactResultHandler,
            IBoardMoveSimulationResultHandler simulationResultHandler)
        {
            _dataAccess = dataAccess;
            _interactResultHandler = interactResultHandler;
            _simulationResultHandler = simulationResultHandler;
        }

        public void InstallEntities()
        {
            var boardData = _dataAccess.GetBoardData();
            _board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _innerPiecesInteractor = new PiecesInteractor.InnerPiecesInteractor(_board);
        }

        public void InstallRefreshRequest(CoreGameplayContainer container)
        {
            var refreshRequester = new RefreshRequester(_board, _dataAccess);
            container.RefreshRequester = refreshRequester;
        }

        public void InstallPiecesInteract(CoreGameplayContainer container)
        {
            container.PiecesInteractor = new PiecesInteractor(_innerPiecesInteractor, _interactResultHandler);
        }

        public void InstallBoardMoveSimulation(CoreGameplayContainer container)
        {
            var simulator = new BoardMoveSimulator(_board, _simulationResultHandler, _innerPiecesInteractor);
            container.BoardMoveSimulator = simulator;
        }

        public void Uninstall(CoreGameplayContainer container)
        {
            container.RefreshRequester = null;
            container.PiecesInteractor = null;
            container.BoardMoveSimulator = null;
        }
    }
}