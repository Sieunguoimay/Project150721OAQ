using Gameplay;
using SNM;
using UnityEngine;

public class PieceToTileSelectorAdaptor : TileSelector.ISelectionAdaptor
{
    private readonly Piece _piece;

    private bool _isDeselected;

    public PieceToTileSelectorAdaptor(Piece piece)
    {
        _piece = piece;
        _isDeselected = false;
    }

    public void OnTileSelected()
    {
        _piece.FaceCamera(false, new Vector3(0, UnityEngine.Random.Range(-25f, 25f), 0));

        _piece.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        PieceScheduler.CreateAAnimActivity(_piece,LegHashes.stand_up,()=>{_piece.Animator.Play(LegHashes.idle);});
        _piece.PieceActivityQueue.Begin();
        
    }

    public void OnTileDeselected(bool success)
    {
        if (!success)
        {
            _piece.Animator.Play(LegHashes.sit_down);
        }
        
        _isDeselected = true;
    }
}