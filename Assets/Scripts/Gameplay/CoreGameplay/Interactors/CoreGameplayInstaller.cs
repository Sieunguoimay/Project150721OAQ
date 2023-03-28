using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInstaller
    {
        private BoardEntity _board;
        private TurnEntity _turnEntity;

        private BoardEntityAccess _boardEntityAccess;
        private readonly ICoreGameplayDataAccess _dataAccess;
        private readonly BoardActionDecisionMakingFactory _decisionMakingFactory;
        private readonly ISimulatorFactory _simulatorFactory;

        public CoreGameplayInstaller(ICoreGameplayDataAccess dataAccess,
            BoardActionDecisionMakingFactory decisionMakingFactory, ISimulatorFactory simulatorFactory)
        {
            _simulatorFactory = simulatorFactory;
            _dataAccess = dataAccess;
            _decisionMakingFactory = decisionMakingFactory;
        }

        public void InstallEntities()
        {
            var boardData = _dataAccess.GetBoardData();
            var turnData = _dataAccess.GetTurnData();

            _board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _turnEntity = CoreEntitiesFactory.CreateTurnEntity(turnData);

            _boardEntityAccess = new BoardEntityAccess(_board);
        }

        public void InstallRefreshRequest(CoreGameplayContainer container)
        {
            container.RefreshRequester = new RefreshRequester(_boardEntityAccess, _dataAccess);
        }

        public void InstallTurnDataExtractor(CoreGameplayContainer container)
        {
            container.TurnDataExtractor = new TurnDataExtractor(_boardEntityAccess, _turnEntity);
        }

        public void InstallSimulatorFactory()
        {
            _simulatorFactory.CreateAllBoardSimulators(_boardEntityAccess);
        }

        public void InstallMoveDecisionMakingDriver(CoreGameplayContainer container)
        {
            container.BoardActionDecisionMakingDriver = new BoardActionDecisionMakingDriver(
                container.TurnDataExtractor, _decisionMakingFactory,
                new BoardActionOptionSequenceFactory(_boardEntityAccess, container.TurnDataExtractor),
                _simulatorFactory);
            container.BoardActionDecisionMakingDriver.InstallDecisionMakings();
        }

        public void InstallGameplayTaskDistributor(CoreGameplayContainer container)
        {
            container.GameplayBranchingDriver = new CoreGameplayInteractDriver(container.TurnDataExtractor,
                container.BoardActionDecisionMakingDriver, _boardEntityAccess);
        }

        public void Uninstall(CoreGameplayContainer container)
        {
            container.BoardActionDecisionMakingDriver.UninstallDecisionMakings();
            container.RefreshRequester = null;
            // container.PiecesInteractor = null;
            // container.BoardMoveSimulator = null;
            // container.GoneWithTheWindSimulator = null;
            // container.ConcurrentBoardMoveSimulator = null;
            container.TurnDataExtractor = null;
            container.BoardActionDecisionMakingDriver = null;
            container.GameplayBranchingDriver = null;

            _board = null;
            _boardEntityAccess = null;
            // _innerPiecesInteractor = null;
        }
    }
}