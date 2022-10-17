using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Piece
{
    // public interface IPieceHolder
    // {
    //     IReadOnlyList<Piece> Pieces { get; }
    //     void Grasp(Piece piece);
    //     void Remove(Piece piece);
    //     void Grasp(IPieceHolder pieceHolder, int count = -1, Action<Piece> onGrasp = null);
    //     void Empty();
    // }
    //
    // public class PieceHolder : IPieceHolder
    // {
    //     private readonly List<Piece> _pieces = new();
    //     public IReadOnlyList<Piece> Pieces => _pieces;
    //
    //     public virtual void Grasp(Piece piece)
    //     {
    //         _pieces.Add(piece);
    //     }
    //
    //     public void Remove(Piece piece)
    //     {
    //         _pieces.Remove(piece);
    //     }
    //
    //     public void Empty()
    //     {
    //         _pieces.Clear();
    //     }
    //
    //     public void Grasp(IPieceHolder other, int count = -1, Action<Piece> onGrasp = null)
    //     {
    //         if (count == other.Pieces.Count || count == -1)
    //         {
    //             foreach (var b in other.Pieces)
    //             {
    //                 _pieces.Add(b);
    //                 onGrasp?.Invoke(b);
    //             }
    //
    //             other.Empty();
    //         }
    //         else
    //         {
    //             var n = Mathf.Min(count, other.Pieces.Count);
    //             var a = other as PieceHolder;
    //             for (var i = n - 1; i >= 0; i--)
    //             {
    //                 _pieces.Add(other.Pieces[i]);
    //                 onGrasp?.Invoke(other.Pieces[i]);
    //                 other.Remove(other.Pieces[i]);
    //             }
    //         }
    //     }
    // }
}