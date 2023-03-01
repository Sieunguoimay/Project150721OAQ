using System;
using System.Collections.Generic;

namespace Gameplay.Visual.Board
{
    public interface IPieceContainer
    {
        IReadOnlyList<Piece.Piece> HeldPieces { get; }
        void AddPiece(Piece.Piece piece);
        void RemovePiece(Piece.Piece piece);
        void Sort(Comparison<Piece.Piece> comparison);
        void Clear();

        public static void TransferAllPiecesOwnership(IPieceContainer @from, IPieceContainer to)
        {
            foreach (var p in @from.HeldPieces)
            {
                to.AddPiece(p);
            }

            @from.Clear();
        }
    }
}