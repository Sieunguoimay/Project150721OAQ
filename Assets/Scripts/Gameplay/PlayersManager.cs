using System.Collections.Generic;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PlayersManager : MonoBehaviour
    {
        public Player[] Players { get; private set; }
        public Player CurrentPlayer => Players[_turn];
        private int _turn = 0;

        public void Setup(Board.TileGroup[] tileGroups, TileSelector tileSelector)
        {
            var n = tileGroups.Length;
            Players = new Player[n];

            for (var i = 0; i < n; i++)
            {
                var pieceBench = new PieceBench(new PieceBench.ConfigData
                {
                    PosAndRot = CalculatePlayerPosition(tileGroups[i]),
                    spacing = 0.25f,
                    perRow = 15
                });

                if (i == n - 1)
                {
                    Players[i] = new RealPlayer(tileGroups[i], pieceBench, tileSelector);
                }
                else
                {
                    Players[i] = new FakePlayer(tileGroups[i], pieceBench, tileSelector);
                }
            }

            CurrentPlayer.AcquireTurn();
        }

        private static PosAndRot CalculatePlayerPosition(Board.TileGroup tg)
        {
            var pos1 = tg.Tiles[0].transform.position;
            var pos2 = tg.Tiles[^1].transform.position;
            var diff = pos2 - pos1;
            var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
            var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
            return new PosAndRot(pos, qua);
        }

        public void ChangePlayer()
        {
            CurrentPlayer.ReleaseTurn();
            _turn = (_turn + 1) % Players.Length;
            CurrentPlayer.AcquireTurn();
        }
    }
}