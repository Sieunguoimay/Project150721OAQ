using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveMoveDecisionMakingDriver : IMoveDecisionMakingResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly IMoveDecisionMakingFactory _factory;
        private readonly BoardMoveSimulator _boardMoveSimulator;
        private readonly ConcurrentMoveSimulator _concurrentMoveSimulator;
        private readonly MoveDecisionOptionFactory _moveDecisionOptionFactory;

        private IMoveDecisionMaking[] _defaultDecisionMakings;
        private IMoveDecisionMaking[] _decisionMakings;
        private ExtractedTurnData CurrentTurnData => _turnDataExtractor.ExtractedTurnData;

        public MoveMoveDecisionMakingDriver(
            TurnDataExtractor turnDataExtractor, IMoveDecisionMakingFactory factory,
            BoardMoveSimulator boardMoveSimulator, 
            ConcurrentMoveSimulator concurrentMoveSimulator, 
            MoveDecisionOptionFactory moveDecisionOptionFactory)
        {
            _turnDataExtractor = turnDataExtractor;
            _factory = factory;
            _boardMoveSimulator = boardMoveSimulator;
            _concurrentMoveSimulator = concurrentMoveSimulator;
            _moveDecisionOptionFactory = moveDecisionOptionFactory;
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
            var decisionMakingData = _moveDecisionOptionFactory.CreateDecisionMakingData(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, this);

            StartCooldownTimer();
        }

        public void OnDecisionResult(MoveDecisionResultData resultData)
        {
            if (resultData.Success)
            {
                StopCooldownTimer();
                // _boardMoveSimulator.RunSimulation(resultData.SingleSimulationInputData);
                _concurrentMoveSimulator.RunSimulation(resultData.SimulationInputData);
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
            var decisionMakingData = _moveDecisionOptionFactory.CreateDecisionMakingData(CurrentTurnData);
            decisionMaking.MakeDecision(decisionMakingData, new DefaultMoveDecisionMakingResultHandler(this));
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
                // _driver._boardMoveSimulator.RunSimulation(resultData.SingleSimulationInputData);
                _driver._concurrentMoveSimulator.RunSimulation(resultData.SimulationInputData);
            }
        }
    }

    public class MoveDecisionResultData
    {
        public bool Success;
        public ConcurrentMoveSimulationInputData SimulationInputData;
        public MoveSimulationInputData SingleSimulationInputData;
    }

    public interface IMoveDecisionMaking
    {
        void MakeDecision(MoveOptionQueue moveOptionQueue, IMoveDecisionMakingResultHandler driver);

        void ForceEnd();

        public static MoveDecisionResultData CreateResultData(MoveOptionQueue moveOptionQueue)
        {
            return new()
            {
                SimulationInputData = CreateConcurrentSimulationInputData(moveOptionQueue),
                SingleSimulationInputData = CreateSimulationInputData(moveOptionQueue),
                Success = true
            };
        }

        public static MoveSimulationInputData CreateSimulationInputData(MoveOptionQueue moveOptionQueue)
        {
            var direction = false;
            var tileIndices = -1;
            foreach (var moveOptionItem in moveOptionQueue.Options)
            {
                var optionValue = moveOptionItem.SelectedValue;
                if (moveOptionItem.OptionItemType == MoveOptionItemType.Direction)
                {
                    direction = ((BooleanOptionValue) optionValue).Value;
                }
                else if (moveOptionItem.OptionItemType == MoveOptionItemType.Tile)
                {
                    tileIndices = (((IntegerOptionValue) optionValue).Value);
                }
            }

            return new()
            {
                Direction = direction,
                SideIndex = moveOptionQueue.TurnIndex,
                StartingTileIndex = tileIndices
            };
        }

        public static ConcurrentMoveSimulationInputData CreateConcurrentSimulationInputData(MoveOptionQueue moveOptionQueue)
        {
            var direction = false;
            var tileIndices = new List<int>();
            foreach (var moveOptionItem in moveOptionQueue.Options)
            {
                var optionValue = moveOptionItem.SelectedValue;
                if (moveOptionItem.OptionItemType == MoveOptionItemType.Direction)
                {
                    direction = ((BooleanOptionValue) optionValue).Value;
                }
                else if (moveOptionItem.OptionItemType == MoveOptionItemType.Tile)
                {
                    tileIndices.Add(((IntegerOptionValue) optionValue).Value);
                }
            }

            return new()
            {
                Direction = direction,
                SideIndex = moveOptionQueue.TurnIndex,
                StartingTileIndices = tileIndices.ToArray()
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