using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveMoveDecisionMakingDriver : IMoveDecisionMakingResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly IMoveDecisionMakingFactory _factory;
        private readonly IBoardMoveSimulator _boardMoveSimulator;
        private readonly MoveDecisionOptionFactory _moveDecisionOptionFactory;

        private IMoveDecisionMaking[] _defaultDecisionMakings;
        private IMoveDecisionMaking[] _decisionMakings;
        private ExtractedTurnData _currentTurnData;

        public MoveMoveDecisionMakingDriver(
            TurnDataExtractor turnDataExtractor, IMoveDecisionMakingFactory factory,
            IBoardMoveSimulator boardMoveSimulator, MoveDecisionOptionFactory moveDecisionOptionFactory)
        {
            _turnDataExtractor = turnDataExtractor;
            _factory = factory;
            _boardMoveSimulator = boardMoveSimulator;
            _moveDecisionOptionFactory = moveDecisionOptionFactory;
        }

        public void InstallDecisionMakings()
        {
            UpdateCurrentTurnData();

            var numTurns = _currentTurnData.NumTurns;

            _defaultDecisionMakings = new IMoveDecisionMaking[numTurns];
            _decisionMakings = new IMoveDecisionMaking[numTurns];

            for (var i = 0; i < numTurns; i++)
            {
                _decisionMakings[i] = i == 0
                    ? _factory.CreatePlayerMoveDecisionMaking()
                    : _factory.CreateComputerMoveDecisionMaking();
                _defaultDecisionMakings[i] = _factory.CreateDefaultMoveDecisionMaking();
            }
        }

        public void UninstallDecisionMakings()
        {
            _decisionMakings = null;
            _defaultDecisionMakings = null;
        }

        public void MakeDecisionOfCurrentTurn()
        {
            var decisionMaking = _decisionMakings[_currentTurnData.CurrentTurnIndex];
            var decisionMakingData = _moveDecisionOptionFactory.CreateDecisionMakingData(_currentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, this);

            StartCooldownTimer();
        }

        public void OnDecisionResult(IMoveDecisionMaking moveDecisionMaking, MoveDecisionResultData resultData)
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
            var decisionMaking = _defaultDecisionMakings[_currentTurnData.CurrentTurnIndex];
            var decisionMakingData = _moveDecisionOptionFactory.CreateDecisionMakingData(_currentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, new DefaultMoveDecisionMakingResultHandler(_boardMoveSimulator));
        }

        public void OnSimulationPresentationEnded()
        {
            // _turnDataExtractor.NextTurn();
            UpdateCurrentTurnData();

            MakeDecisionOfCurrentTurn();
        }

        private void UpdateCurrentTurnData()
        {
            _currentTurnData = _turnDataExtractor.ExtractTurnData();
        }

        private class DefaultMoveDecisionMakingResultHandler : IMoveDecisionMakingResultHandler
        {
            private readonly IBoardMoveSimulator _boardMoveSimulator;

            public DefaultMoveDecisionMakingResultHandler(IBoardMoveSimulator boardMoveSimulator)
            {
                _boardMoveSimulator = boardMoveSimulator;
            }

            public void OnDecisionResult(IMoveDecisionMaking moveDecisionMaking, MoveDecisionResultData resultData)
            {
                _boardMoveSimulator.RunSimulation(resultData.SimulationInputData);
            }
        }
    }

    public class MoveDecisionResultData
    {
        public bool Success;
        public MoveSimulationInputData SimulationInputData;
    }

    public interface IMoveDecisionMaking
    {
        void MakeDecision(MoveDecisionMakingData moveDecisionMakingData, IMoveDecisionMakingResultHandler driver);

        void ForceEnd();
    }

    public interface IMoveDecisionMakingResultHandler
    {
        void OnDecisionResult(IMoveDecisionMaking moveDecisionMaking, MoveDecisionResultData resultData);
    }

    public interface IMoveDecisionMakingFactory
    {
        IMoveDecisionMaking CreateDefaultMoveDecisionMaking();
        IMoveDecisionMaking CreatePlayerMoveDecisionMaking();
        IMoveDecisionMaking CreateComputerMoveDecisionMaking();
    }
}