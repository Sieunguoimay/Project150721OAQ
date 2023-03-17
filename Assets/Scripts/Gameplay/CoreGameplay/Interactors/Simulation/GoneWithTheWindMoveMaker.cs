using System;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class GoneWithTheWindMoveMaker : MoveMaker
    {
        private bool _firstGraspFlag;

        public GoneWithTheWindMoveMaker(string id, BoardEntityAccess boardEntityAccess) : base(id, boardEntityAccess)
        {
        }

        public override void SetStartingCondition(int sideIndex, int startingTileIndex, bool direction)
        {
            base.SetStartingCondition(sideIndex, startingTileIndex, direction);
            _firstGraspFlag = true;
        }

        public override void Grasp(Action doneHandler)
        {
            if (_firstGraspFlag && TileIterator.NextTile.TileType != TileType.MandarinTile)
            {
                _firstGraspFlag = false;

                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.CurrentTile,
                    TempPieceContainer);
                PiecesInteractor.InnerPiecesInteractor.MoveAllPiecesFromContainerToContainer(TileIterator.NextTile,
                    TempPieceContainer);

                FinalizeMove(MoveType.DoubleGrasp);
                doneHandler?.Invoke();
            }
            else
            {
                base.Grasp(doneHandler);
            }
        }
    }
}