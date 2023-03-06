using UnityEngine;

namespace Gameplay.Visual
{
    public static class BoardStateMatchVisualVerify
    {
        public static void Verify(BoardStateView boardStateView, Board.Board boardVisual)
        {
            for (var i = 0; i < boardStateView.RefreshData.PiecesInTiles.Length; i++)
            {
                var visualCount = boardVisual.Tiles[i].HeldPieces.Count;
                var dataCount = boardStateView.RefreshData.PiecesInTiles[i].CitizenPiecesCount;
                if (visualCount == dataCount)
                {
                    Debug.Log("Tile Matching " + visualCount);
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