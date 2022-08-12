using System;
using System.Collections.Generic;
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

        public void AssignPieceBench(Board.Board board)
        {
            foreach (var p in Players)
            {
                p.PieceBench = board.GetPieceBench(p.Index);
            }
        }
    }
}