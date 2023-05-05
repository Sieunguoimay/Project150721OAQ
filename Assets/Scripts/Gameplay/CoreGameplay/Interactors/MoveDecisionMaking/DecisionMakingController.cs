using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DecisionMakingController :
        SelfBindingDependencyInversionUnit,
        IDecisionMakingResultHandler
    {
        private TurnDataExtractor _turnDataExtractor;
        private IDecisionMakingFactory _factory;
        private OptionSequenceFactory _boardActionOptionSequenceFactory;

        private IDecisionMaker[] _defaultDecisionMakings;
        private IDecisionMaker[] _decisionMakings;
        private ISimulatorFactory _simulatorFactory;
        private ExtractedTurnData CurrentTurnData => _turnDataExtractor.ExtractedTurnData;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _simulatorFactory = Resolver.Resolve<ISimulatorFactory>();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
            _factory = Resolver.Resolve<IDecisionMakingFactory>();
            _boardActionOptionSequenceFactory = Resolver.Resolve<OptionSequenceFactory>();
        }

        public void InstallDecisionMakings()
        {
            var numTurns = CurrentTurnData.NumTurns;

            _defaultDecisionMakings = new IDecisionMaker[numTurns];
            _decisionMakings = new IDecisionMaker[numTurns];

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
            MakeDecision(CurrentTurnData.CurrentTurnIndex);
        }

        private void MakeDecision(int turnIndex)
        {
            var decisionMaking = _decisionMakings[turnIndex];
            var optionQueue = _boardActionOptionSequenceFactory.CreateOptionSequence();
            decisionMaking.MakeDecision(new DecisionMakingData
            {
                OptionQueue = optionQueue,
                ActionType = SimulationType.Basic
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
            _decisionMakings[CurrentTurnData.CurrentTurnIndex].Cancel();

            HandleDecisionMakingFailed();
        }

        private void HandleDecisionMakingFailed()
        {
            MakeDecisionByDefault(CurrentTurnData.CurrentTurnIndex);
        }

        private void MakeDecisionByDefault(int turnIndex)
        {
            var decisionMaking = _defaultDecisionMakings[turnIndex];
            var decisionMakingData = _boardActionOptionSequenceFactory.CreateOptionSequence();
            decisionMaking.MakeDecision(new DecisionMakingData
            {
                OptionQueue = decisionMakingData,
                ActionType = SimulationType.Basic
            }, new DefaultBoardActionDecisionMakingResultHandler(this));
        }

        private void RunSimulation(BoardActionDecisionResultData resultData)
        {
            _simulatorFactory.GetSimulator(resultData.ActionType).RunSimulation(resultData.SimulationInputData);
        }

        private class DefaultBoardActionDecisionMakingResultHandler : IDecisionMakingResultHandler
        {
            private readonly DecisionMakingController _driver;

            public DefaultBoardActionDecisionMakingResultHandler(DecisionMakingController driver)
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
        public SimulationType ActionType;
    }

    public enum SimulationType
    {
        Basic,
        GoneWithTheWind,
        Concurrent,
    }

    public class BoardActionData
    {
        public SimulationType ActionType;
        public IBoardMoveSimulator Simulator;
        public OptionQueue OptionQueue;
    }

    public interface IDecisionMakingResultHandler
    {
        void OnDecisionResult(BoardActionDecisionResultData resultData);
    }

    public interface IDecisionMakingFactory
    {
        IDecisionMaker CreateDefaultDecisionMaking();
        IDecisionMaker CreatePlayerDecisionMaking();
        IDecisionMaker CreateComputerDecisionMaking();
    }
}