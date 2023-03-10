using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.DecisionMaking;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInstaller
    {
        private BoardEntity _board;
        private TurnEntity _turnEntity;
        private BoardEntityAccess _boardEntityAccess;
        private PiecesInteractor.InnerPiecesInteractor _innerPiecesInteractor;

        private readonly ICoreGameplayDataAccess _dataAccess;
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
            var turnData = _dataAccess.GetTurnData();

            _board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _turnEntity = CoreEntitiesFactory.CreateTurnEntity(turnData);

            _boardEntityAccess = new BoardEntityAccess(_board);
            _innerPiecesInteractor = new PiecesInteractor.InnerPiecesInteractor(_boardEntityAccess);
        }

        public void InstallRefreshRequest(CoreGameplayContainer container)
        {
            container.RefreshRequester = new RefreshRequester(_boardEntityAccess, _dataAccess);
        }

        public void InstallPiecesInteract(CoreGameplayContainer container)
        {
            container.PiecesInteractor = new PiecesInteractor(_innerPiecesInteractor, _interactResultHandler);
        }

        public void InstallBoardMoveSimulation(CoreGameplayContainer container)
        {
            container.BoardMoveSimulator = new BoardMoveSimulator(_board, _simulationResultHandler, _boardEntityAccess);
        }

        public void InstallTurnDataExtractor(CoreGameplayContainer container)
        {
            container.TurnDataExtractor = new TurnDataExtractor(_boardEntityAccess, _turnEntity);
        }

        public void InstallGameplayDriver(CoreGameplayContainer container)
        {
            container.GameplayDriver = new GameplayDriver(container.TurnDataExtractor, new DecisionMakingFactory(), container.BoardMoveSimulator);
            container.GameplayDriver.InstallDecisionMakings();
        }

        public void Uninstall(CoreGameplayContainer container)
        {
            container.GameplayDriver.UninstallDecisionMakings();
            container.RefreshRequester = null;
            container.PiecesInteractor = null;
            container.BoardMoveSimulator = null;
            container.TurnDataExtractor = null;
            container.GameplayDriver = null;

            _board = null;
            _boardEntityAccess = null;
            _innerPiecesInteractor = null;
        }
    }
}