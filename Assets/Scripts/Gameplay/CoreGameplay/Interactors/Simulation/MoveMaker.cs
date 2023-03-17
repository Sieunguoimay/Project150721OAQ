using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class MoveMaker : IMoveMaker, MoveInnerRules<TileEntity>.IMoveRuleDataHelper
    {
        private readonly string _id;
        private Action<MoveMaker, MoveSimulationProgressData> _progressHandler;
        private readonly BoardEntityAccess _boardEntityAccess;
        protected readonly PieceContainerEntity TempPieceContainer = new() { PieceEntities = new List<PieceEntity>() };

        private int _sideIndex;

        public MoveMaker(string id, BoardEntityAccess boardEntityAccess)
        {
            _id = id;
            _boardEntityAccess = boardEntityAccess;
            MoveInnerRules = new MoveInnerRules<TileEntity>(this);
        }

        public void SetProgressHandler(Action<MoveMaker, MoveSimulationProgressData> progressHandler)
        {
            _progressHandler = progressHandler;
        }

        public virtual void SetStartingCondition(int sideIndex, int startingTileIndex, bool direction)
        {
            _sideIndex = sideIndex;
            TileIterator = new TileIterator<TileEntity>(_boardEntityAccess.TileEntities, direction);
            TileIterator.UpdateCurrentTileIndex(startingTileIndex);
        }

        public virtual void Grasp(Action doneHandler)
        {
            // Debug.Log($"{_id} Grasp {TileIterator.CurrentTileIndex}");
            PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                TempPieceContainer);

            FinalizeMove(MoveType.Grasp);
            doneHandler?.Invoke();
        }

        public void Drop(Action doneHandler)
        {
            // Debug.Log($"{_id} Drop {TileIterator.CurrentTileIndex}");
            PiecesInteractor.InnerPiecesInteractor.MoveSinglePieceFromContainerToContainer(TempPieceContainer,
                TileIterator.CurrentTile);

            FinalizeMove(MoveType.Drop);
            doneHandler?.Invoke();
        }

        public void Slam(Action doneHandler)
        {
            // Debug.Log($"{_id} Slam");
            FinalizeMove(MoveType.Slam);
            doneHandler?.Invoke();
        }

        public void Eat(Action doneHandler)
        {
            // Debug.Log($"{_id} Eat");
            var pocket = _boardEntityAccess.GetPocketAtIndex(_sideIndex);
            PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                pocket);
            FinalizeMove(MoveType.Eat);
            doneHandler?.Invoke();
        }

        protected void FinalizeMove(MoveType moveType)
        {
            _progressHandler?.Invoke(this, CreateOutput(moveType));
            TileIterator.UpdateCurrentTileIndex(
                Array.IndexOf(_boardEntityAccess.TileEntities, TileIterator.NextTile));
        }

        private MoveSimulationProgressData CreateOutput(MoveType moveType)
        {
            return new MoveSimulationProgressData
            {
                MoveType = moveType,
                TileIndex = TileIterator.CurrentTileIndex,
                NextTileIndex =  Array.IndexOf(_boardEntityAccess.TileEntities, TileIterator.NextTile),
            };
        }

        public bool CanDrop()
        {
            return TempPieceContainer.PieceEntities.Count > 0;
        }

        public IMoveInnerRules MoveInnerRules { get; }

        public TileIterator<TileEntity> TileIterator { get; private set; }

        public int GetNumPiecesInTile(TileEntity tile)
        {
            return tile.PieceEntities.Count;
        }

        public bool IsMandarinTile(TileEntity tile)
        {
            return Array.IndexOf(_boardEntityAccess.TileEntities, tile) %
                (_boardEntityAccess.TileEntities.Length / 2) == 0;
        }
    }
}