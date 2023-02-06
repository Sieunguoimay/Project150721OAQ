﻿using System.Linq;
using Common.Activity;
using Gameplay;
using Gameplay.Board;
using Gameplay.GameInteract;

namespace System
{
    public interface IGameplay
    {
        void StartNewMatch();
    }

    public class Gameplay : IGameplay
    {
        private readonly IPieceDropper _dropper = new PieceDropper();
        private readonly PieceEater _eater = new();

        private Player[] _players;
        private Board _board;
        private GameInteractManager _interact;

        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }

        public ActivityQueue ActivityQueue { get; } = new();

        public void Setup(Player[] players, Board board, GameInteractManager interactManager)
        {
            _board = board;
            _players = players;
            _interact = interactManager;
            _eater.SetBoard(_board);

            IsPlaying = false;
            IsGameOver = false;
            NextPlayer();
            _interact.MoveEvent -= OnMove;
            _interact.MoveEvent += OnMove;
        }


        public void StartNewMatch()
        {
            IsPlaying = true;
            _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles);
        }

        public void ClearGame()
        {
            IsGameOver = false;
            IsPlaying = false;
            _dropper.Cleanup();
            _eater.Cleanup();
            CurrentPlayer = null;
            _interact.ResetAll();
            _interact.MoveEvent -= OnMove;
        }

        #region PRIVATE_METHODS

        private void NextPlayer()
        {
            if (CurrentPlayer == null)
            {
                CurrentPlayer = _players[0];
            }
            else
            {
                CurrentPlayer.ReleaseTurn();
                CurrentPlayer = _players[(CurrentPlayer.Index + 1) % _players.Length];
            }

            CurrentPlayer.AcquireTurn();
        }


        private void OnMove(ITile tile, bool forward)
        {
            _dropper.Take(_board, tile, tile.HeldPieces.Count);
            _dropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tile), forward);
            _dropper.DropTillDawn(lastTile =>
            {
                _eater.SetUpForEating(CurrentPlayer.PieceBench, forward, MakeDecision);
                _eater.EatRecursively(Board.GetSuccessTile(_board.Tiles, lastTile, forward));
            });
        }

        private void MakeDecision()
        {
            var allMandarinTilesEmpty = _board.Sides.All(tg => tg.MandarinTile.HeldPieces.Count <= 0);
            if (allMandarinTilesEmpty)
            {
                GameOver();
                return;
            }

            NextPlayer();

            var tileGroup = _board.Sides[CurrentPlayer.Index];
            var isNewPlayerAllEmpty = tileGroup.CitizenTiles.All(t => t.HeldPieces.Count <= 0);
            if (isNewPlayerAllEmpty)
            {
                if (CurrentPlayer.PieceBench.HeldPieces.Count <= 0)
                {
                    GameOver();
                    return;
                }

                //Take back pieces to board
                _dropper.Take(_board, CurrentPlayer.PieceBench, tileGroup.CitizenTiles.Length);
                _dropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), true);
                _dropper.DropOnce(_ =>
                {
                    _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles);
                });
                return;
            }

            _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles);
        }

        private void GameOver()
        {
            EvaluateWinner();

            if (IsGameOver) return;

            IsGameOver = true;
            IsPlaying = false;
        }

        private void EvaluateWinner()
        {
            for (var i = 0; i < _players.Length; i++)
            {
                foreach (var tile in _board.Sides[i].CitizenTiles)
                {
                    // _players[i].PieceBench.HeldPieces.AddRange(tile.HeldPieces);
                    tile.Clear();
                }

                // var sum = _players[i].PieceBench.HeldPieces.Sum(p => p is Citizen ? 1 : 10);

                // _perMatchData.SetPlayerScore(i, sum);
            }

            // TellWinner(_perMatchData.PlayerScores);
        }

        // private static void TellWinner(IReadOnlyList<Currency> scores)
        // {
        //     if (scores[0].Get() > scores[1].Get())
        //     {
        //         Debug.Log("Player 1 win! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        //     else if (scores[0].Get() < scores[1].Get())
        //     {
        //         Debug.Log("Player 2 win! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        //     else
        //     {
        //         Debug.Log("Draw! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        // }

        #endregion
    }
}