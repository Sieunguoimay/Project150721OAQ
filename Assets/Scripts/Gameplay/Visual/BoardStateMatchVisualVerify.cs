using Gameplay.Visual.Board;
using Gameplay.Visual.Presenters;
using UnityEngine;

namespace Gameplay.Visual
{
    public class BoardStateMatchVisualVerify
    {
        private readonly BoardStateView _boardStateView;
        private readonly BoardVisual _boardVisualVisual;

        public BoardStateMatchVisualVerify(BoardStateView boardStateView, BoardVisual boardVisualVisual)
        {
            _boardStateView = boardStateView;
            _boardVisualVisual = boardVisualVisual;
        }

        public void Verify()
        {
            for (var i = 0; i < _boardStateView.RefreshData.PiecesInTiles.Length; i++)
            {
                var visualCount = _boardVisualVisual.TileVisuals[i].HeldPieces.Count;
                var piecesInTile = _boardStateView.RefreshData.PiecesInTiles[i];
                var dataCount = piecesInTile.CitizenPiecesCount + piecesInTile.MandarinPiecesCount;
                if (visualCount == dataCount)
                {
                    // Debug.Log("Tile Matching " + visualCount);
                }
                else
                {
                    Debug.LogError("Board visual and data not matched");
                    return;
                }
            }
        }
    }
}