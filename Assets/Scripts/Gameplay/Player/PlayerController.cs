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
        // void CreatePieceBench(Board.Board board);
    }
    
    public class PlayerController : BaseGenericDependencyInversionUnit<PlayerController>,IPlayerManager
    {
        [field: System.NonSerialized] public IReadOnlyList<Player> Players { get; private set; }
        public Player CurrentPlayer { get; private set; }
        private Player _mainPlayer;
        public event Action PlayerChangedEvent;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _mainPlayer = new Player(0);
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

        public void ChangePlayer(Player newPlayer)
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
                    players[i] = new Player(i);
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

    }
}