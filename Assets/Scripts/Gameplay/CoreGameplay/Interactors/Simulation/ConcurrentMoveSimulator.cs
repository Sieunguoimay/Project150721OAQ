using System;
using System.Linq;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class ConcurrentMoveSimulator : IRefreshResultHandler
    {
        private readonly IBoardMoveSimulationResultHandler _simulationResultHandler;
        private readonly BoardEntityAccess _boardEntityAccess;
        private MultiBoardStateMachine _boardStateMachine;
        private MoveMaker _moveMaker;
        private MoveMaker[] _moveMakers;
        private readonly ICoreGameplayController _controller;
        private int _simulationId;
        private readonly IRefreshRequester _requester;

        public ConcurrentMoveSimulator(IBoardMoveSimulationResultHandler resultHandler, BoardEntityAccess boardEntityAccess, IRefreshRequester requester)
        {
            _requester = requester;
            _simulationResultHandler = resultHandler;
            _boardEntityAccess = boardEntityAccess;
        }

        public void RunSimulation(ConcurrentMoveSimulationInputData simulationInputData)
        {
            _moveMakers = new MoveMaker[simulationInputData.StartingTileIndices.Length];
            for (var i = 0; i < simulationInputData.StartingTileIndices.Length; i++)
            {
                _moveMakers[i] = new MoveMaker($"{i}", OnSimulationProgress, _boardEntityAccess);
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
            _simulationId = Array.IndexOf(_moveMakers, moveMaker);
            _simulationResultHandler.OnSimulationProgress(_simulationId, result);
            _requester.Refresh(this);
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            var str = $"{_simulationId}: ";
            str = refreshData.PiecesInTiles.Aggregate(str, (current, pieces) => current + $"{pieces.CitizenPiecesCount + pieces.MandarinPiecesCount} ");
            Debug.Log(str);
        }
    }

    public class ConcurrentMoveSimulationInputData
    {
        public int[] StartingTileIndices;
        public bool Direction;
        public int SideIndex;
    }
}