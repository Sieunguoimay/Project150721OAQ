using UnityEngine;

namespace Gameplay.Piece
{
    public interface IMandarin : IPiece
    {
    }

    [SelectionBase]
    public class Mandarin : Piece, IMandarin
    {
    }
}