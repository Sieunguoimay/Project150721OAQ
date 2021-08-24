using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class PlayersManager
    {
        public class StateData
        {
            public int turn = 0;
        }

        private StateData state = new StateData();

        private Player[] players;

        public Player[] Players => players;
        public Player CurrentPlayer => Players[state.turn];

        public void Setup(List<Board.TileGroup> tileGroups, TileSelector tileSelector)
        {
            int n = tileGroups.Count;
            players = new Player[n];

            for (int i = 0; i < n; i++)
            {
                var location = CalculatePlayerPosition(tileGroups[i]);
                var pieceBench = SNM.Utils.NewGameObject<PieceBench>();
                pieceBench.transform.SetPositionAndRotation(location.Item1, location.Item2);

                if (true) //i == n - 1)
                {
                    players[i] = new RealPlayer(tileGroups[i], pieceBench, tileSelector);
                }
                else
                {
                    players[i] = new Player(tileGroups[i], pieceBench);
                }
            }

            CurrentPlayer.AcquireTurn();
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

        public void ChangePlayer()
        {
            CurrentPlayer.ReleaseTurn();
            state.turn = (state.turn + 1) % Players.Length;
            CurrentPlayer.AcquireTurn();
        }
    }
}