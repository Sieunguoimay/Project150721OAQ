using System;
using System.Collections.Generic;

namespace Gameplay.Visual.Board
{
    public interface IPieceContainer
    {
        IReadOnlyList<Piece.PieceVisual> HeldPieces { get; }
        void AddPiece(Piece.PieceVisual pieceVisual);
        void RemovePiece(Piece.PieceVisual pieceVisual);
        void Sort(Comparison<Piece.PieceVisual> comparison);
        void Clear();

        public static void TransferAllPiecesOwnership(IPieceContainer @from, IPieceContainer to)
        {
            foreach (var p in @from.HeldPieces)
            {
                to.AddPiece(p);
            }

            @from.Clear();
        }

        public static void TransferPiecesOwnerShip(IPieceContainer from, IPieceContainer to, int amount)
        {
            var citizens = from.HeldPieces;
            var n = citizens.Count;
            for (var i = 0; i < amount; i++)
            {
                var index = n - i - 1;

                to.AddPiece(citizens[index]);
                from.RemovePiece(citizens[index]);
            }
        }
    }
}