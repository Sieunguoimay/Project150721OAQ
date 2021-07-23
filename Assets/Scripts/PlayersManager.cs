using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class PlayersManager
    {
        private Player[] players;

        public Player[] Players => players;

        public void Setup(List<Board.TileGroup> tileGroups)
        {
            int n = tileGroups.Count;
            players = new Player[n];

            for (int i = 0; i < n; i++)
            {
                var location = CalculatePlayerPosition(tileGroups[i]);

                players[i].pieceBench = SNM.Utils.NewGameObject<PieceBench>();
                players[i].pieceBench.transform.SetPositionAndRotation(location.Item1, location.Item2);
                players[i].tileGroup = tileGroups[i];
            }
        }

        private (Vector3, Quaternion) CalculatePlayerPosition(Board.TileGroup tg)
        {
            var pos1 = tg.tiles[0].transform.position;
            var pos2 = tg.tiles[tg.tiles.Count - 1].transform.position;
            var diff = pos2 - pos1;
            var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
            var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
            return (pos, qua);
        }
    }
}