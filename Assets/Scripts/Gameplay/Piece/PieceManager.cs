using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace InGame
{
    public class PieceManager
    {
        private readonly Piece _mandarinPrefab;
        private readonly Piece _citizenPrefab;
        
        public PieceManager(Piece mandarinPrefab,Piece citizenPrefab)
        {
            _mandarinPrefab = mandarinPrefab;
            _citizenPrefab = citizenPrefab;
        }
        
        public void SpawnPieces(Board board)
        {
            var container = new GameObject("Container");
            container.transform.SetParent(board.transform);
            container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            for (var i = 0; i < board.Tiles.Length; i++)
            {
                var t = board.Tiles[i];
                if (t is MandarinTile)
                {
                    var tg = new Board.TileGroup {MandarinTile = t, Tiles = new List<Tile>()};
                    Board.InitializeTileGroup(ref tg);
                    board.TileGroups.Add(tg);

                    var m = Object.Instantiate(_mandarinPrefab) as Mandarin;
                    m.Setup();
                    t.Grasp(m);
                    t.Reposition(m.transform);
                }
                else
                {
                    for (var j = 0; j < 5; j++)
                    {
                        var b = Object.Instantiate(_citizenPrefab, container.transform, true) as Citizen;
                        b.Setup();
                        t.Grasp(b);
                        t.Reposition(b.transform);
                    }
                }
            }
        }
    }
}