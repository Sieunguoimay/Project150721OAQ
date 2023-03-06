using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public interface IBoardMoveSimulator
    {
        void RunSimulation(MoveSimulationInputData inputData);
    }

    public class BoardMoveSimulator : IBoardMoveSimulator
    {
        private readonly IBoardMoveSimulationResultHandler _refreshResultHandler;
        private readonly IBoardStateDriver _boardStateMachine;
        private readonly MoveMaker _moveMaker;

        public BoardMoveSimulator(BoardEntity board, IBoardMoveSimulationResultHandler resultHandler,
            PiecesInteractor.InnerPiecesInteractor piecesInteractor)
        {
            _refreshResultHandler = resultHandler;
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
            _refreshResultHandler.OnSimulationResult(new MoveSimulationResultData());
        }

        private class MoveMaker : IMoveMaker, MoveInnerRules<TileEntity>.IMoveRuleDataHelper
        {
            private MoveSimulationInputData _moveConfig;
            private readonly BoardEntity _boardEntity;
            private readonly IBoardMoveSimulationResultHandler _resultHandler;
            private readonly PiecesInteractor.InnerPiecesInteractor _piecesInteractor;

            private readonly PieceContainerEntity _tempPieceContainer = new() {PieceEntities = new List<PieceEntity>()};

            public MoveMaker(BoardEntity boardEntity, IBoardMoveSimulationResultHandler resultHandler,
                PiecesInteractor.InnerPiecesInteractor piecesInteractor)
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
                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                    _tempPieceContainer);

                FinalizeMove(MoveType.Grasp);
                doneHandler?.Invoke();
            }

            public void Drop(Action doneHandler)
            {
                PiecesInteractor.InnerPiecesInteractor.MoveSinglePieceFromContainerToContainer(_tempPieceContainer,
                    TileIterator.CurrentTile);

                FinalizeMove(MoveType.Drop);
                doneHandler?.Invoke();
            }

            public void Slam(Action doneHandler)
            {
                FinalizeMove(MoveType.Slam);
                doneHandler?.Invoke();
            }

            public void Eat(Action doneHandler)
            {
                var pocket = _boardEntity.Sides[_moveConfig.SideIndex].Pocket;
                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                    pocket);
                FinalizeMove(MoveType.Eat);
                doneHandler?.Invoke();
            }

            private void FinalizeMove(MoveType moveType)
            {
                var numCitizens =
                    TileIterator.CurrentTile.PieceEntities.Count(p => p.PieceType == PieceType.Citizen);
                _resultHandler?.OnSimulationProgress(CreateOutput(moveType, numCitizens));
                TileIterator.UpdateCurrentTileIndex(
                    Array.IndexOf(_piecesInteractor.TileEntities, TileIterator.NextTile));
            }

            private MoveSimulationProgressData CreateOutput(MoveType moveType, int numCitizens)
            {
                return new()
                {
                    MoveType = moveType,
                    TileIndex = TileIterator.CurrentTileIndex,
                    NumCitizens = numCitizens
                };
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
                return Array.IndexOf(_piecesInteractor.TileEntities, tile) %
                    (_piecesInteractor.TileEntities.Length / 2) == 0;
            }
        }
    }

    public class MoveSimulationInputData
    {
        public int StartingTileIndex;
        public bool Direction;
        public int SideIndex;
    }

    public class MoveSimulationResultData
    {
        public RefreshData RefreshData;
    }

    public class MoveSimulationProgressData
    {
        public MoveType MoveType;
        public int TileIndex;
        public int NumCitizens;
    }

    public enum MoveType
    {
        Grasp,
        Drop,
        Slam,
        Eat
    }

    public interface IBoardMoveSimulationResultHandler
    {
        void OnSimulationResult(MoveSimulationResultData result);
        void OnSimulationProgress(MoveSimulationProgressData result);
    }
}