using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Board;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public interface IBoardMoveSimulator
    {
        void RunSimulation(MoveSimulationInputData inputData);
    }

    public class BoardMoveSimulator : IBoardMoveSimulator
    {
        private readonly IBoardMoveSimulationResultHandler _resultHandler;
        private readonly BoardStateMachine _boardStateMachine;
        private readonly MoveMaker _moveMaker;

        public BoardMoveSimulator(BoardEntity board, IBoardMoveSimulationResultHandler resultHandler, PiecesInteractor.InnerPiecesInteractor piecesInteractor)
        {
            _resultHandler = resultHandler;
            _moveMaker = new MoveMaker(board, resultHandler, piecesInteractor);
            _boardStateMachine = new BoardStateMachine(_moveMaker);
            _boardStateMachine.EndEvent += OnBoardStateMachineEnd;
        }

        public void RunSimulation(MoveSimulationInputData inputData)
        {
            _moveMaker.Initialize(inputData);
            _boardStateMachine.NextAction();
        }

        private void OnBoardStateMachineEnd(IBoardStateDriver obj)
        {
            var resultData = new MoveSimulationOutputData();
            _resultHandler.OnSimulationResult(resultData);
        }

        private class MoveMaker : IMoveMaker, MoveInnerRules<TileEntity>.IMoveRuleDataHelper
        {
            private MoveSimulationInputData _moveConfig;
            private readonly BoardEntity _boardEntity;
            private readonly IBoardMoveSimulationResultHandler _resultHandler;
            private readonly PiecesInteractor.InnerPiecesInteractor _piecesInteractor;

            private readonly PieceContainerEntity _tempPieceContainer = new() {PieceEntities = new List<PieceEntity>()};

            public MoveMaker(BoardEntity boardEntity, IBoardMoveSimulationResultHandler resultHandler, PiecesInteractor.InnerPiecesInteractor piecesInteractor)
            {
                _boardEntity = boardEntity;
                _resultHandler = resultHandler;
                _piecesInteractor = piecesInteractor;
                MoveInnerRules = new MoveInnerRules<TileEntity>(this);
            }

            public void Initialize(MoveSimulationInputData moveConfig)
            {
                _moveConfig = moveConfig;
                TileIterator = new TileIterator<TileEntity>(_piecesInteractor.TileEntities, moveConfig.Direction);
                TileIterator.UpdateCurrentTileIndex(_moveConfig.StartingTileIndex);
            }

            public void Grasp(Action doneHandler)
            {
                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile, _tempPieceContainer);

                TileIterator.UpdateCurrentTileIndex(Array.IndexOf(_piecesInteractor.TileEntities, TileIterator.NextTile));
                _resultHandler?.OnSimulationProgress(new MoveSimulationOutputData());
                doneHandler?.Invoke();
            }

            public void Drop(Action doneHandler)
            {
                PiecesInteractor.InnerPiecesInteractor.MoveSinglePieceFromContainerToContainer(_tempPieceContainer, TileIterator.CurrentTile);

                TileIterator.UpdateCurrentTileIndex(Array.IndexOf(_piecesInteractor.TileEntities, TileIterator.NextTile));
                _resultHandler?.OnSimulationProgress(new MoveSimulationOutputData());
                doneHandler?.Invoke();
            }

            public void Slam(Action doneHandler)
            {
                TileIterator.UpdateCurrentTileIndex(Array.IndexOf(_piecesInteractor.TileEntities, TileIterator.NextTile));
                _resultHandler?.OnSimulationProgress(new MoveSimulationOutputData());
                doneHandler?.Invoke();
            }

            public void Eat(Action doneHandler)
            {
                var pocket = _boardEntity.Sides[_moveConfig.SideIndex].Pocket;
                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile, pocket);

                TileIterator.UpdateCurrentTileIndex(Array.IndexOf(_piecesInteractor.TileEntities, TileIterator.NextTile));
                _resultHandler?.OnSimulationProgress(new MoveSimulationOutputData());
                doneHandler?.Invoke();
            }

            public bool CanDrop()
            {
                return _tempPieceContainer.PieceEntities.Count > 0;
            }

            public IMoveInnerRules MoveInnerRules { get; }

            public TileIterator<TileEntity> TileIterator { get; private set; }

            public int GetNumPiecesInTile(TileEntity tile)
            {
                return tile.PieceEntities.Count;
            }

            public bool IsMandarinTile(TileEntity tile)
            {
                return Array.IndexOf(_piecesInteractor.TileEntities, tile) % (_piecesInteractor.TileEntities.Length / 2) == 0;
            }
        }
    }

    public class MoveSimulationInputData
    {
        public int StartingTileIndex;
        public bool Direction;
        public int SideIndex;
    }

    public class MoveSimulationOutputData
    {
    }

    public interface IBoardMoveSimulationResultHandler
    {
        void OnSimulationResult(MoveSimulationOutputData result);
        void OnSimulationProgress(MoveSimulationOutputData result);
    }
}