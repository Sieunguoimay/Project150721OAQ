using System;
using Gameplay.Visual.Board;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class ConcurrentMoveSimulator
    {
        private readonly IBoardMoveSimulationResultHandler _simulationResultHandler;
        private readonly BoardEntityAccess _boardEntityAccess;
        private MultiBoardStateMachine _boardStateMachine;
        private MoveMaker _moveMaker;
        private MoveMaker[] _moveMakers;

        public ConcurrentMoveSimulator(IBoardMoveSimulationResultHandler resultHandler, BoardEntityAccess boardEntityAccess)
        {
            _simulationResultHandler = resultHandler;
            _boardEntityAccess = boardEntityAccess;
        }

        public void RunSimulation(ConcurrentMoveSimulationInputData simulationInputData)
        {
            _moveMakers = new MoveMaker[simulationInputData.StartingTileIndices.Length];
            for (var i = 0; i < simulationInputData.StartingTileIndices.Length; i++)
            {
                _moveMakers[i] = new MoveMaker(OnSimulationProgress, _boardEntityAccess);
                _moveMakers[i].Initialize(simulationInputData.SideIndex, simulationInputData.StartingTileIndices[i], simulationInputData.Direction);
            }

            _boardStateMachine = new MultiBoardStateMachine(_moveMakers);
            
            _boardStateMachine.SetEndHandler(OnBoardStateMachineEnd);
            _boardStateMachine.NextAction();
        }

        private void OnBoardStateMachineEnd()
        {
            _simulationResultHandler.OnSimulationResult(new MoveSimulationResultData());
        }

        private void OnSimulationProgress(MoveMaker moveMaker, MoveSimulationProgressData result)
        {
            _simulationResultHandler.OnSimulationProgress(Array.IndexOf(_moveMakers, moveMaker), result);
        }
    }

    public class ConcurrentMoveSimulationInputData
    {
        public int[] StartingTileIndices;
        public bool Direction;
        public int SideIndex;
    }
}