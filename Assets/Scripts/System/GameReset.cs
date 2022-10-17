using System.Collections.Generic;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;

namespace System
{
    // public class GameReset
    // {
    //     private readonly Board _board;
    //     private readonly PlayersManager _playersManager;
    //     public Action OnDone = delegate { };
    //
    //     public GameReset(Board board, PlayersManager playersManager)
    //     {
    //         _playersManager = playersManager;
    //         _board = board;
    //     }
    //
    //     public void Reset()
    //     {
    //         var citizens = new List<Piece>();
    //         var mandarins = new List<Piece>();
    //         foreach (var player in _playersManager.Players)
    //         {
    //             foreach (var p in player.PieceBench.Pieces)
    //             {
    //                 if (p is Citizen)
    //                 {
    //                     citizens.Add(p);
    //                 }
    //                 else
    //                 {
    //                     mandarins.Add(p);
    //                 }
    //             }
    //
    //             player.PieceBench.Pieces.Clear();
    //         }
    //
    //         foreach (var tile in _board.Tiles)
    //         {
    //             switch (tile)
    //             {
    //                 case MandarinTile mt:
    //                     tile.Grasp(mandarins, Math.Max(0, 1 - tile.Pieces.Count), p => mt.Reposition(p.transform));
    //                     break;
    //                 case Tile tt:
    //                     tile.Grasp(citizens, 5, p => tt.Reposition(p.transform));
    //                     break;
    //             }
    //         }
    //     }
    // }
}