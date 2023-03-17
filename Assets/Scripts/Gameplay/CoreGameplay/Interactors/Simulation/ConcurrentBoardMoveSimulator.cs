using System;
using System.Linq;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class ConcurrentBoardMoveSimulator : IBoardMoveSimulator
    {
        private readonly IConcurrentMoveSimulationResultHandler _simulationResultHandler;
        private readonly BoardEntityAccess _boardEntityAccess;

        private MultiBoardStateMachine _boardStateMachine;
        private MoveMaker[] _moveMakers;

        private readonly ICoreGameplayController _controller;
        private int _simulationId;

        public ConcurrentBoardMoveSimulator(IConcurrentMoveSimulationResultHandler resultHandler, BoardEntityAccess boardEntityAccess)
        {
            _simulationResultHandler = resultHandler;
            _boardEntityAccess = boardEntityAccess;
        }

        public void RunSimulation(MoveSimulationInputData simulationInputData)
        {
            _moveMakers = new MoveMaker[simulationInputData.StartingTileIndices.Length];
            for (var i = 0; i < simulationInputData.StartingTileIndices.Length; i++)
            {
                _moveMakers[i] = new MoveMaker($"{i}", _boardEntityAccess);
                _moveMakers[i].SetProgressHandler(OnSimulationProgress);
                _moveMakers[i].SetStartingCondition(simulationInputData.SideIndex, simulationInputData.StartingTileIndices[i], simulationInputData.Direction);
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
            _simulationId = Array.IndexOf(_moveMakers, moveMaker);
            _simulationResultHandler.OnSimulationProgress(_simulationId, result);
        }
    }

    public class MoveSimulationInputData
    {
        public int StartingTileIndex;
        public int[] StartingTileIndices;
        public bool Direction;
        public int SideIndex;
    }

    public interface IConcurrentMoveSimulationResultHandler
    {
        void OnSimulationProgress(int threadId, MoveSimulationProgressData result);
        void OnSimulationResult(MoveSimulationResultData result);
    }
}