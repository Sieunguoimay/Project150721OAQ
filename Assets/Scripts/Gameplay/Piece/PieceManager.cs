using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace InGame
{
    public class PieceManager : MonoBehaviour
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        public void SpawnPieces(Board board)
        {
            for (var i = 0; i < board.Tiles.Length; i++)
            {
                var t = board.Tiles[i];
                if (t is MandarinTile)
                {
                    var tg = new Board.TileGroup {MandarinTile = t, Tiles = new List<Tile>()};
                    Board.InitializeTileGroup(ref tg);
                    board.TileGroups.Add(tg);

                    var m = Instantiate(mandarinPrefab) as Mandarin;
                    m.Setup();
                    t.Grasp(m);
                    var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1), false);
                    m.PieceActivityQueue.Add(new Flocking(m.Config.flockingConfigData, new Flocking.InputData() {target = position, transform = m.transform}, null));
                    // t.Reposition(m.transform);
                }
                else
                {
                    for (var j = 0; j < 5; j++)
                    {
                        var b = Instantiate(citizenPrefab, transform, true) as Citizen;
                        b.Setup();
                        t.Grasp(b);
                        // t.Reposition(b.transform);
                        var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1), false);
                        b.PieceActivityQueue.Add(new Flocking(b.Config.flockingConfigData, new Flocking.InputData() {target = position, transform = b.transform}, null));
                    }
                }
            }
        }
    }
}