using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class MoveMaker : IMoveMaker, MoveInnerRules<TileEntity>.IMoveRuleDataHelper
    {
        private readonly string _id;
        private readonly Action<MoveMaker, MoveSimulationProgressData> _progressHandler;
        private readonly BoardEntityAccess _boardEntityAccess;
        private readonly PieceContainerEntity _tempPieceContainer = new() {PieceEntities = new List<PieceEntity>()};

        private int _sideIndex;

        public MoveMaker(string id,Action<MoveMaker, MoveSimulationProgressData> progressHandler, BoardEntityAccess boardEntityAccess)
        {
            _id = id;
            _progressHandler = progressHandler;
            _boardEntityAccess = boardEntityAccess;
            MoveInnerRules = new MoveInnerRules<TileEntity>(this);
        }

        public void Initialize(int sideIndex, int startingTileIndex, bool direction)
        {
            _sideIndex = sideIndex;
            TileIterator = new TileIterator<TileEntity>(_boardEntityAccess.TileEntities, direction);
            TileIterator.UpdateCurrentTileIndex(startingTileIndex);
        }

        public void Grasp(Action doneHandler)
        {
            // Debug.Log($"{_id} Grasp {TileIterator.CurrentTileIndex}");
            PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                _tempPieceContainer);

            FinalizeMove(MoveType.Grasp);
            doneHandler?.Invoke();
        }

        public void Drop(Action doneHandler)
        {
            // Debug.Log($"{_id} Drop {TileIterator.CurrentTileIndex}");
            PiecesInteractor.InnerPiecesInteractor.MoveSinglePieceFromContainerToContainer(_tempPieceContainer,
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

        private void FinalizeMove(MoveType moveType)
        {
            var numCitizens =
                TileIterator.CurrentTile.PieceEntities.Count(p => p.PieceType == PieceType.Citizen);
            var numMandarins =
                TileIterator.CurrentTile.PieceEntities.Count(p => p.PieceType == PieceType.Mandarin);

            _progressHandler?.Invoke(this, CreateOutput(moveType, numCitizens, numMandarins));
            TileIterator.UpdateCurrentTileIndex(
                Array.IndexOf(_boardEntityAccess.TileEntities, TileIterator.NextTile));
        }

        private MoveSimulationProgressData CreateOutput(MoveType moveType, int numCitizens, int numMandarins)
        {
            return new()
            {
                MoveType = moveType,
                TileIndex = TileIterator.CurrentTileIndex,
                NumCitizens = numCitizens,
                NumMandarins = numMandarins,
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
            return Array.IndexOf(_boardEntityAccess.TileEntities, tile) %
                (_boardEntityAccess.TileEntities.Length / 2) == 0;
        }
    }
}