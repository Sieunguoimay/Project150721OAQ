using Gameplay.Board;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PlayersManager : MonoBehaviour
    {
        public Player[] Players { get; private set; }
        private Player _mainPlayer;

        private void Start()
        {
            _mainPlayer = new RealPlayer(0);
        }

        public void FillWithFakePlayers(int n)
        {
            Players = new Player[n];
            Players[0] = _mainPlayer;
            for (var i = 1; i < n; i++)
            {
                Players[i] = new FakePlayer(i);
            }
        }

        public void CreatePieceBench(Board.Board board)
        {
            foreach (var p in Players)
            {
                p.PieceBench = new PieceBench(new PieceBench.ConfigData
                {
                    PosAndRot = CalculatePieceBenchPosition(board.TileGroups[p.Index]),
                    spacing = 0.25f,
                    perRow = 15
                });
            }

            static PosAndRot CalculatePieceBenchPosition(Board.Board.TileGroup tg)
            {
                var pos1 = ((Tile) tg.Tiles[0]).transform.position;
                var pos2 = ((Tile) tg.Tiles[^1]).transform.position;
                var diff = pos2 - pos1;
                var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
                var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
                return new PosAndRot(pos, qua);
            }
        }
    }
}