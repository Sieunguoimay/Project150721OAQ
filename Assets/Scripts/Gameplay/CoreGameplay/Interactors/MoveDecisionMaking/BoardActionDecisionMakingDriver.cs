using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class BoardActionDecisionMakingDriver :
        SelfBindingDependencyInversionUnit,
        IBoardActionDecisionMakingResultHandler
    {
        private TurnDataExtractor _turnDataExtractor;
        private IBoardActionDecisionMakingFactory _factory;
        private BoardActionOptionSequenceFactory _boardActionOptionSequenceFactory;

        private IBoardActionDecisionMaking[] _defaultDecisionMakings;
        private IBoardActionDecisionMaking[] _decisionMakings;
        private ISimulatorFactory _simulatorFactory;
        private ExtractedTurnData CurrentTurnData => _turnDataExtractor.ExtractedTurnData;

        private BoardActionData[] _boardActionDataList;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _simulatorFactory = Resolver.Resolve<ISimulatorFactory>();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
            _factory = Resolver.Resolve<IBoardActionDecisionMakingFactory>();
            _boardActionOptionSequenceFactory = Resolver.Resolve<BoardActionOptionSequenceFactory>();
        }

        public void InstallDecisionMakings()
        {
            var numTurns = CurrentTurnData.NumTurns;

            _defaultDecisionMakings = new IBoardActionDecisionMaking[numTurns];
            _decisionMakings = new IBoardActionDecisionMaking[numTurns];

            for (var i = 0; i < numTurns; i++)
            {
                // _decisionMakings[i] = i == 0 ? _factory.CreatePlayerMoveDecisionMaking() : _factory.CreateComputerMoveDecisionMaking();
                _decisionMakings[i] = _factory.CreatePlayerDecisionMaking();
                _defaultDecisionMakings[i] = _factory.CreateDefaultDecisionMaking();
            }
        }

        public void UninstallDecisionMakings()
        {
            _decisionMakings = null;
            _defaultDecisionMakings = null;
        }

        public void MakeDecisionOfCurrentTurn()
        {
            var decisionMaking = _decisionMakings[CurrentTurnData.CurrentTurnIndex];
            var optionQueue = _boardActionOptionSequenceFactory.CreateOptionSequence();
            decisionMaking.MakeDecision(new DecisionMakingData
            {
                OptionQueue = optionQueue,
                ActionType = BoardActionType.Basic
            }, this);

            StartCooldownTimer();
        }

        public void OnDecisionResult(BoardActionDecisionResultData resultData)
        {
            if (resultData.Success)
            {
                StopCooldownTimer();
                RunSimulation(resultData);
            }
            else
            {
                HandleDecisionMakingFailed();
            }
        }

        private void StartCooldownTimer()
        {
            //Todo
        }

        private void StopCooldownTimer()
        {
            //Todo
        }

        public void OnCooldownTimerEnded()
        {
            _decisionMakings[CurrentTurnData.CurrentTurnIndex].ForceEnd();

            HandleDecisionMakingFailed();
        }

        private void HandleDecisionMakingFailed()
        {
            MakeDecisionByDefault();
        }

        private void MakeDecisionByDefault()
        {
            var decisionMaking = _defaultDecisionMakings[CurrentTurnData.CurrentTurnIndex];
            var decisionMakingData = _boardActionOptionSequenceFactory.CreateOptionSequence();
            decisionMaking.MakeDecision(new DecisionMakingData
            {
                OptionQueue = decisionMakingData,
                ActionType = BoardActionType.Basic
            }, new DefaultBoardActionDecisionMakingResultHandler(this));
        }

        private void RunSimulation(BoardActionDecisionResultData resultData)
        {
            _simulatorFactory.GetSimulator(resultData.ActionType).RunSimulation(resultData.SimulationInputData);
        }

        private class DefaultBoardActionDecisionMakingResultHandler : IBoardActionDecisionMakingResultHandler
        {
            private readonly BoardActionDecisionMakingDriver _driver;

            public DefaultBoardActionDecisionMakingResultHandler(BoardActionDecisionMakingDriver driver)
            {
                _driver = driver;
            }

            public void OnDecisionResult(BoardActionDecisionResultData resultData)
            {
                _driver.RunSimulation(resultData);
            }
        }
    }

    public class BoardActionDecisionResultData
    {
        public bool Success;
        public MoveSimulationInputData SimulationInputData;
        public BoardActionType ActionType;
    }

    public enum BoardActionType
    {
        Basic,
        GoneWithTheWind,
        Concurrent,
    }

    public class BoardActionData
    {
        public BoardActionType ActionType;
        public IBoardMoveSimulator Simulator;
        public OptionQueue OptionQueue;
    }

    public interface IBoardActionDecisionMakingResultHandler
    {
        void OnDecisionResult(BoardActionDecisionResultData resultData);
    }

    public interface IBoardActionDecisionMakingFactory
    {
        IBoardActionDecisionMaking CreateDefaultDecisionMaking();
        IBoardActionDecisionMaking CreatePlayerDecisionMaking();
        IBoardActionDecisionMaking CreateComputerDecisionMaking();
    }
}