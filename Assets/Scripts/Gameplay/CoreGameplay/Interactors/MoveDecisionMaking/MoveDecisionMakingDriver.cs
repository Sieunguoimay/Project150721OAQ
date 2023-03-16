using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveMoveDecisionMakingDriver : IMoveDecisionMakingResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly IMoveDecisionMakingFactory _factory;
        private readonly BoardMoveSimulator _boardMoveSimulator;
        private readonly ConcurrentBoardMoveSimulator _concurrentBoardMoveSimulator;
        private readonly MoveOptionSequenceFactory _moveOptionSequenceFactory;

        private IMoveDecisionMaking[] _defaultDecisionMakings;
        private IMoveDecisionMaking[] _decisionMakings;
        private ExtractedTurnData CurrentTurnData => _turnDataExtractor.ExtractedTurnData;

        public MoveMoveDecisionMakingDriver(
            TurnDataExtractor turnDataExtractor, IMoveDecisionMakingFactory factory,
            BoardMoveSimulator boardMoveSimulator,
            ConcurrentBoardMoveSimulator concurrentBoardMoveSimulator,
            MoveOptionSequenceFactory moveOptionSequenceFactory)
        {
            _turnDataExtractor = turnDataExtractor;
            _factory = factory;
            _boardMoveSimulator = boardMoveSimulator;
            _concurrentBoardMoveSimulator = concurrentBoardMoveSimulator;
            _moveOptionSequenceFactory = moveOptionSequenceFactory;
        }

        public void InstallDecisionMakings()
        {
            var numTurns = CurrentTurnData.NumTurns;

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
            var decisionMaking = _decisionMakings[CurrentTurnData.CurrentTurnIndex];
            var decisionMakingData = _moveOptionSequenceFactory.CreateMoveOptionSequence(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, this);

            StartCooldownTimer();
        }

        public void OnDecisionResult(MoveDecisionResultData resultData)
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
            var decisionMakingData = _moveOptionSequenceFactory.CreateMoveOptionSequence(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, new DefaultMoveDecisionMakingResultHandler(this));
        }

        private void RunSimulation(MoveDecisionResultData resultData)
        {
            Debug.Log("RunSimulation");
            if (resultData.IsConcurrentSimulation)
            {
                _concurrentBoardMoveSimulator.RunSimulation(resultData.ConcurrentSimulationInputData);
            }
            else
            {
                _boardMoveSimulator.RunSimulation(resultData.ConcurrentSimulationInputData);
            }
        }

        private class DefaultMoveDecisionMakingResultHandler : IMoveDecisionMakingResultHandler
        {
            private readonly MoveMoveDecisionMakingDriver _driver;

            public DefaultMoveDecisionMakingResultHandler(MoveMoveDecisionMakingDriver driver)
            {
                _driver = driver;
            }

            public void OnDecisionResult(MoveDecisionResultData resultData)
            {
                _driver.RunSimulation(resultData);
            }
        }
    }

    public class MoveDecisionResultData
    {
        public bool Success;
        public ConcurrentMoveSimulationInputData ConcurrentSimulationInputData;
        public bool IsConcurrentSimulation;
    }

    public interface IMoveDecisionMaking
    {
        void MakeDecision(OptionQueue optionQueue, IMoveDecisionMakingResultHandler driver);

        void ForceEnd();

        public static MoveDecisionResultData CreateResultData(OptionQueue optionQueue)
        {
            var concurrentMoveSimulationInputData = CreateConcurrentSimulationInputData(optionQueue);
            return new()
            {
                ConcurrentSimulationInputData = concurrentMoveSimulationInputData,
                Success = true,
                IsConcurrentSimulation = concurrentMoveSimulationInputData.StartingTileIndices.Length > 0,
            };
        }

        public static ConcurrentMoveSimulationInputData CreateConcurrentSimulationInputData(
            OptionQueue optionQueue)
        {
            var direction = false;
            var tileIndices = new List<int>();
            foreach (var moveOptionItem in optionQueue.Options)
            {
                switch (moveOptionItem)
                {
                    case DirectionOptionItem directionOptionItem:
                        direction = directionOptionItem.SelectedDirection;
                        break;
                    case TileOptionItem tileOptionItem:
                        tileIndices.Add(tileOptionItem.SelectedTileIndex);
                        break;
                }
            }

            if (tileIndices.Count == 0)
            {
                Debug.LogError("Must provide a tileOption");
            }

            return new ConcurrentMoveSimulationInputData
            {
                Direction = direction,
                SideIndex = optionQueue.TurnIndex,
                StartingTileIndices = tileIndices.ToArray(),
                StartingTileIndex = tileIndices[0],
            };
        }
    }

    public interface IMoveDecisionMakingResultHandler
    {
        void OnDecisionResult(MoveDecisionResultData resultData);
    }

    public interface IMoveDecisionMakingFactory
    {
        IMoveDecisionMaking CreateDefaultMoveDecisionMaking();
        IMoveDecisionMaking CreatePlayerMoveDecisionMaking();
        IMoveDecisionMaking CreateComputerMoveDecisionMaking();
    }
}