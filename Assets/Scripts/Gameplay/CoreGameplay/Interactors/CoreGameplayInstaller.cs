using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

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
        private readonly MoveDecisionMakingFactory _decisionMakingFactory;

        public CoreGameplayInstaller(ICoreGameplayDataAccess dataAccess,
            IPieceInteractResultHandler interactResultHandler,
            IBoardMoveSimulationResultHandler simulationResultHandler,
            MoveDecisionMakingFactory decisionMakingFactory)
        {
            _dataAccess = dataAccess;
            _interactResultHandler = interactResultHandler;
            _simulationResultHandler = simulationResultHandler;
            _decisionMakingFactory = decisionMakingFactory;
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

        public void InstallMoveDecisionMakingDriver(CoreGameplayContainer container)
        {
            container.MoveMoveDecisionMakingDriver = new MoveMoveDecisionMakingDriver(
                container.TurnDataExtractor, _decisionMakingFactory,
                container.BoardMoveSimulator, new MoveDecisionOptionFactory(_boardEntityAccess));
            container.MoveMoveDecisionMakingDriver.InstallDecisionMakings();
        }

        public void InstallGameplayTaskDistributor(CoreGameplayContainer container)
        {
            container.GameplayBranchingDriver = new CoreGameplayInteractDriver(container.TurnDataExtractor,
                container.MoveMoveDecisionMakingDriver, _boardEntityAccess);
        }

        public void Uninstall(CoreGameplayContainer container)
        {
            container.MoveMoveDecisionMakingDriver.UninstallDecisionMakings();
            container.RefreshRequester = null;
            container.PiecesInteractor = null;
            container.BoardMoveSimulator = null;
            container.TurnDataExtractor = null;
            container.MoveMoveDecisionMakingDriver = null;
            container.GameplayBranchingDriver = null;

            _board = null;
            _boardEntityAccess = null;
            _innerPiecesInteractor = null;
        }
    }
}