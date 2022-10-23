﻿using Common.ResolveSystem;
using Gameplay.Board;
using Gameplay.GameInteract;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PlayersManager : InjectableBehaviour<PlayersManager>
    {
        public Player[] Players { get; private set; }
        private Player _mainPlayer;
        private GameInteractManager _interactManager;

        public override void Setup(IResolver resolver)
        {
            _interactManager = resolver.Resolve<GameInteractManager>();
        }

        public override void TearDown()
        {
        }

        private void Start()
        {
            _mainPlayer = new Player(0, _interactManager);
        }

        public void ResetAll()
        {
            foreach (var player in Players)
            {
                player.ResetAll();
            }
        }

        public void FillWithFakePlayers(int n)
        {
            Players = new Player[n];
            Players[0] = _mainPlayer;
            for (var i = 1; i < n; i++)
            {
                Players[i] = new Player(i, _interactManager);
            }
        }

        public void CreatePieceBench(Board.Board board)
        {
            foreach (var p in Players)
            {
                var tg = board.TileGroups[p.Index];
                var pos1 = tg.Tiles[0].transform.position;
                var pos2 = tg.Tiles[^1].transform.position;
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