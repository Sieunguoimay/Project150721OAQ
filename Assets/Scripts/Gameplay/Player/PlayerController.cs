using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Player
{
    public interface IPlayerManager
    {
        IReadOnlyList<Gameplay.Player.Player> Players { get; }
        Gameplay.Player.Player CurrentPlayer { get; }
        void NextPlayer();
        void ChangePlayer(Gameplay.Player.Player newPlayer);
        void FillUpWithFakePlayers(int n);
        void DeletePlayers();
  
        PieceBench GetCurrentPlayerBench();
        void CreatePieceBench(Board.Board board);
    }
    
    public class PlayerController : BaseGenericDependencyInversionUnit<PlayerController>,IPlayerManager
    {
        [field: System.NonSerialized] public IReadOnlyList<Gameplay.Player.Player> Players { get; private set; }
        public Gameplay.Player.Player CurrentPlayer { get; private set; }
        private Gameplay.Player.Player _mainPlayer;
        public event Action PlayerChangedEvent;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _mainPlayer = new Gameplay.Player.Player(0);
        }

        public PieceBench GetCurrentPlayerBench()
        {
            return CurrentPlayer.PieceBench;
        }

        public void NextPlayer()
        {
            var nextPlayerIndex = CurrentPlayer == null ? 0 : (CurrentPlayer.Index + 1) % Players.Count;
            ChangePlayer(Players[nextPlayerIndex]);
        }

        public void ChangePlayer(Gameplay.Player.Player newPlayer)
        {
            CurrentPlayer?.ReleaseTurn();
            CurrentPlayer = newPlayer;
            CurrentPlayer.AcquireTurn();
            PlayerChangedEvent?.Invoke();
        }

        public void FillUpWithFakePlayers(int n)
        {
            var players = new Gameplay.Player.Player[n];
            for (var i = 0; i < n; i++)
            {
                if (i == 0)
                {
                    players[i] = _mainPlayer;
                }
                else
                {
                    players[i] = new Gameplay.Player.Player(i);
                }
            }

            Players = players;
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
                var pos1 = tg.CitizenTiles[0].Transform.position;
                var pos2 = tg.CitizenTiles[^1].Transform.position;
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