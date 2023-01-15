using UnityEngine;

namespace Gameplay
{
    public class PlayersManager : MonoControlUnitBase<PlayersManager>
    {
        [field: System.NonSerialized] public Player[] Players { get; private set; }
        private Player _mainPlayer;
        protected override void OnSetup()
        {
            base.OnSetup();
            _mainPlayer = new Player(0);
        }

        public void FillWithFakePlayers(int n)
        {
            Players = new Player[n];
            for (var i = 0; i < n; i++)
            {
                if (i == 0)
                {
                    Players[i] = _mainPlayer;
                }
                else
                {
                    Players[i] = new Player(i);
                }
            }
        }

        public void DeletePlayers()
        {
            foreach (var p in Players)
            {
                Destroy(p.PieceBench.gameObject);
            }
            Players = null;
        }
        public void CreatePieceBench(Board.Board board)
        {
            foreach (var p in Players)
            {
                var tg = board.Sides[p.Index];
                var pos1 = tg.CitizenTiles[0].transform.position;
                var pos2 = tg.CitizenTiles[^1].transform.position;
                var diff = pos2 - pos1;
                var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
                var rot = Quaternion.LookRotation(pos1 - pos, Vector3.up);

                p.PieceBench = new GameObject($"{nameof(PieceBench)} {p.Index}").AddComponent<PieceBench>();
                p.PieceBench.SetArrangement(0.25f, 15);

                var t = p.PieceBench.transform;
                t.SetParent(transform);
                t.position = pos;
                t.rotation = rot;
            }
        }

    }
}