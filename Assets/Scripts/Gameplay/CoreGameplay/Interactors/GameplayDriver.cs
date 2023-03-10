using System;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class GameplayDriver : IDecisionMakingResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly IDecisionMakingFactory _factory;
        private readonly IBoardMoveSimulator _boardMoveSimulator;
        private IDecisionMaking[] _defaultDecisionMakings;
        private IDecisionMaking[] _decisionMakings;
        private ExtractedTurnData _currentTurnData;

        public GameplayDriver(TurnDataExtractor turnDataExtractor, IDecisionMakingFactory factory, IBoardMoveSimulator boardMoveSimulator)
        {
            _turnDataExtractor = turnDataExtractor;
            _factory = factory;
            _boardMoveSimulator = boardMoveSimulator;
        }

        public void InstallDecisionMakings()
        {
            UpdateCurrentTurnData();

            var numTurns = _currentTurnData.NumTurns;

            _defaultDecisionMakings = new IDecisionMaking[numTurns];
            _decisionMakings = new IDecisionMaking[numTurns];

            for (var i = 0; i < numTurns; i++)
            {
                _decisionMakings[i] = i == 0 ? _factory.CreatePlayerDecisionMaking() : _factory.CreateComputerDecisionMaking();
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
            _decisionMakings[_currentTurnData.CurrentTurnIndex].MakeDecision(_currentTurnData.DecisionMakingData, this);

            StartCooldownTimer();
        }

        public void OnDecisionResult(IDecisionMaking decisionMaking, DecisionResultData resultData)
        {
            if (resultData.Success)
            {
                StopCooldownTimer();
                _boardMoveSimulator.RunSimulation(resultData.SimulationInputData);
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
            _decisionMakings[_currentTurnData.CurrentTurnIndex].ForceEnd();

            HandleDecisionMakingFailed();
        }

        private void HandleDecisionMakingFailed()
        {
            MakeDecisionByDefault();
        }

        private void MakeDecisionByDefault()
        {
            _defaultDecisionMakings[_currentTurnData.CurrentTurnIndex].MakeDecision(_currentTurnData.DecisionMakingData, new DefaultDecisionMakingResultHandler(_boardMoveSimulator));
        }

        public void OnSimulationPresentationEnded()
        {
            _turnDataExtractor.NextTurn();
            UpdateCurrentTurnData();
            MakeDecisionOfCurrentTurn();
        }

        private void UpdateCurrentTurnData()
        {
            _currentTurnData = _turnDataExtractor.ExtractTurnData();
        }

        private class DefaultDecisionMakingResultHandler : IDecisionMakingResultHandler
        {
            private readonly IBoardMoveSimulator _boardMoveSimulator;

            public DefaultDecisionMakingResultHandler(IBoardMoveSimulator boardMoveSimulator)
            {
                _boardMoveSimulator = boardMoveSimulator;
            }

            public void OnDecisionResult(IDecisionMaking decisionMaking, DecisionResultData resultData)
            {
                _boardMoveSimulator.RunSimulation(resultData.SimulationInputData);
            }
        }
    }

    public class DecisionResultData
    {
        public bool Success;
        public MoveSimulationInputData SimulationInputData;
    }

    public class DecisionOption
    {
        public int TileIndex;
    }

    public class DecisionMakingData
    {
        public DecisionOption[] Options;
        public int TurnIndex;
    }

    public interface IDecisionMaking
    {
        void MakeDecision(DecisionMakingData decisionMakingData, IDecisionMakingResultHandler driver);

        void ForceEnd();
    }

    public interface IDecisionMakingResultHandler
    {
        void OnDecisionResult(IDecisionMaking decisionMaking, DecisionResultData resultData);
    }

    public interface IDecisionMakingFactory
    {
        IDecisionMaking CreateDefaultDecisionMaking();
        IDecisionMaking CreatePlayerDecisionMaking();
        IDecisionMaking CreateComputerDecisionMaking();
    }
}