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
        _piece.Delay(UnityEngine.Random.Range(0, 0.5f), () =>
            {
                if (!_isDeselected)
                {
                    // _piece.Animator.CrossFade("jump", 0.1f);
                }
            }
        );
    }

    public void OnTileDeselected()
    {
        // _piece.Animator.CrossFade("idle", 0.1f);
        _isDeselected = true;
    }
}