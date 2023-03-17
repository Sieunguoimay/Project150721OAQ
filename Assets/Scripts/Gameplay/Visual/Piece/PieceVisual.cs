using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceVisual : MonoBehaviour
    {
        public IPieceContainer CurrentPieceContainer { get; private set; }

        public void SetCurrentPieceContainer(IPieceContainer pieceContainer)
        {
            CurrentPieceContainer = pieceContainer;
        }
    }
}