using System;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class BoardActionBoardActionDecisionMakingDriver : IBoardActionDecisionMakingResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly IBoardActionDecisionMakingFactory _factory;
        private readonly CoreGameplayContainer _container;

        private readonly BoardActionOptionSequenceFactory _boardActionOptionSequenceFactory;

        private IBoardActionDecisionMaking[] _defaultDecisionMakings;
        private IBoardActionDecisionMaking[] _decisionMakings;
        private ExtractedTurnData CurrentTurnData => _turnDataExtractor.ExtractedTurnData;

        public BoardActionBoardActionDecisionMakingDriver(
            TurnDataExtractor turnDataExtractor, IBoardActionDecisionMakingFactory factory,
            CoreGameplayContainer container,
            BoardActionOptionSequenceFactory boardActionOptionSequenceFactory)
        {
            _turnDataExtractor = turnDataExtractor;
            _factory = factory;
            _container = container;
            _boardActionOptionSequenceFactory = boardActionOptionSequenceFactory;
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
            var decisionMakingData = _boardActionOptionSequenceFactory.CreateOptionSequence(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, this);

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
            var decisionMakingData = _boardActionOptionSequenceFactory.CreateOptionSequence(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, new DefaultBoardActionDecisionMakingResultHandler(this));
        }

        private void RunSimulation(BoardActionDecisionResultData resultData)
        {
            Debug.Log("RunSimulation");
            GetSimulator(resultData.ActionType).RunSimulation(resultData.SimulationInputData);
        }

        private IBoardMoveSimulator GetSimulator(BoardActionType actionType)
        {
            return actionType switch
            {
                BoardActionType.Basic => _container.BoardMoveSimulator,
                BoardActionType.GoneWithTheWind => _container.GoneWithTheWindSimulator,
                BoardActionType.Concurrent => _container.ConcurrentBoardMoveSimulator,
                _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
            };
        }

        private class DefaultBoardActionDecisionMakingResultHandler : IBoardActionDecisionMakingResultHandler
        {
            private readonly BoardActionBoardActionDecisionMakingDriver _driver;

            public DefaultBoardActionDecisionMakingResultHandler(BoardActionBoardActionDecisionMakingDriver driver)
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