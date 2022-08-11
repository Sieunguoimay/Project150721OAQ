using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Piece
{
    public class CitizenToTileSelectorAdaptor : TileSelector.ISelectionAdaptor
    {
        private readonly Citizen _piece;

        public CitizenToTileSelectorAdaptor(Citizen piece)
        {
            _piece = piece;
        }

        public void OnTileSelected()
        {
            _piece.FaceCamera(false, new Vector3(0, UnityEngine.Random.Range(-25f, 25f), 0));

            PieceScheduler.CreateAAnimActivity(_piece,LegHashes.stand_up,()=>{_piece.Animator.Play(LegHashes.idle);});
            _piece.PieceActivityQueue.Begin();
        
        }

        public void OnTileDeselected(bool success)
        {
            if (!success)
            {
                _piece.Animator.Play(LegHashes.sit_down);
            }
        }
    }
}