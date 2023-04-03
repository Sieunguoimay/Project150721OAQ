using System;
using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Controllers
{
    public interface ICoreGameplayController
    {
        void SetupNewGame();
        void TearDownGame();
        void RunGameplay();
        void CheckBranching();
        void RequestRefresh(IRefreshResultHandler resultPresenter);
    }

    public class CoreGameplayController : 
        SelfBindingGenericDependencyInversionUnit<ICoreGameplayController>, 
        ICoreGameplayController
    {
        private BoardActionDecisionMakingDriver _decisionMakingDriver;
        private IRefreshRequester _refreshRequester;
        private CoreGameplayBranchingDriver _branchingDriver;
        private ICoreGameplayDataAccess _dataAccess;
        private BoardEntityAccess _boardEntityAccess;
        private TurnDataExtractor _turnDataExtractor;
        private ISimulatorFactory _simulatorFactory;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            
            _refreshRequester = Resolver.Resolve<IRefreshRequester>();
            _dataAccess = Resolver.Resolve<ICoreGameplayDataAccess>();
            _simulatorFactory = Resolver.Resolve<ISimulatorFactory>();
            _decisionMakingDriver = Resolver.Resolve<BoardActionDecisionMakingDriver>();
            _branchingDriver = Resolver.Resolve<CoreGameplayBranchingDriver>();
            _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
        }

        public void SetupNewGame()
        {
            SetupEntities();
            SetupSimulatorFactory();
            SetupTurnDataExtractor();
            SetupDecisionMakingDriver();
        }

        public void TearDownGame()
        {
            _decisionMakingDriver.UninstallDecisionMakings();
        }

        private void SetupEntities()
        {
            _dataAccess.RefreshData();
            
            var boardData = _dataAccess.GetBoardData();
            var board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _boardEntityAccess.SetBoardEntity(board);
        }

        private void SetupTurnDataExtractor()
        {
            var turnData = _dataAccess.GetTurnData();
            var turnEntity = CoreEntitiesFactory.CreateTurnEntity(turnData);
            _turnDataExtractor.SetTurnEntity(turnEntity);
        }

        private void SetupSimulatorFactory()
        {
            _simulatorFactory.CreateAllBoardSimulators();
        }

        private void SetupDecisionMakingDriver()
        {
            _decisionMakingDriver.InstallDecisionMakings();
        }
        

        public void RunGameplay()
        {
            _decisionMakingDriver.MakeDecisionOfCurrentTurn();
        }

        public void CheckBranching()
        {
            _branchingDriver.CheckBranching();
        }

        public void RequestRefresh(IRefreshResultHandler resultPresenter)
        {
            _refreshRequester.Refresh(resultPresenter);
        }
    }
}