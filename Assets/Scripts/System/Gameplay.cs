﻿using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Activity;
using Gameplay;
using Gameplay.Board;
using Gameplay.Entities;
using Gameplay.GameInteract;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using SNM;
using UnityEngine;

namespace System
{
    public interface IGameplay
    {
        void StartNewMatch();
    }

    public class Gameplay : IGameplay
    {
        private readonly PieceDropper _dropper = new();
        private readonly PieceEater _eater = new();

        private PlayersManager _playersManager;
        private Board _board;
        private PieceManager _pieceManager;
        private GameInteractManager _interact;

        // private PerMatchData _perMatchData;
        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }

        public ActivityQueue ActivityQueue { get; } = new();

        public void Setup(PlayersManager playersManager, Board board, PieceManager pieceManager,
            GameInteractManager interactManager)
        {
            _board = board;
            _playersManager = playersManager;
            _pieceManager = pieceManager;
            _interact = interactManager;

            _dropper.SetBoard(_board);
            _eater.SetBoard(_board);

            IsPlaying = false;
            IsGameOver = false;
        }
        
        public void StartNewMatch()
        {
            IsPlaying = true;
            ChangePlayer();
            // _perMatchData = new PerMatchData(_playersManager.Players.Length);
            // foreach (var score in _perMatchData.PlayerScores)
            // {
            //     score.Inject(_resolver);
            // }

            _pieceManager.ReleasePieces(() =>
            {
                _interact.SetupInteract(_board.Sides[CurrentPlayer.Index], new MoveCommand(this),
                    new MoveCommand(this));
                _interact.ShowTileChooser();
            }, _board);
        }

        public void ClearGame()
        {
            IsGameOver = false;
            IsPlaying = false;
            _dropper.ClearHoldingPieces();
            _eater.Cleanup();
            CurrentPlayer = null;
        }

        #region PRIVATE_METHODS

        private void ChangePlayer()
        {
            if (CurrentPlayer == null)
            {
                CurrentPlayer = _playersManager.Players[0];
            }
            else
            {
                CurrentPlayer.ReleaseTurn();
                CurrentPlayer = _playersManager.Players[(CurrentPlayer.Index + 1) % _playersManager.Players.Length];
            }

            CurrentPlayer.AcquireTurn();
        }


        private class MoveCommand : GameInteractManager.MoveButtonCommand
        {
            private readonly Gameplay _gameplay;

            public MoveCommand(Gameplay gameplay)
            {
                _gameplay = gameplay;
            }

            protected override void Move(Tile tile, bool forward)
            {
                _gameplay._dropper.Take(tile.PiecesContainer, tile.PiecesContainer.Count);
                _gameplay._dropper.SetMoveStartPoint(Array.IndexOf(_gameplay._board.Tiles, tile), forward);
                _gameplay._dropper.DropTillDawn(lastTile =>
                {
                    _gameplay._eater.SetUpForEating(_gameplay.CurrentPlayer.PieceBench, forward,
                        _gameplay.MakeDecision);
                    _gameplay._eater.EatRecursively(_gameplay._board.GetSuccessTile(lastTile, forward));
                });
            }
        }

        private void MakeDecision()
        {
            var allMandarinTilesEmpty = _board.Sides.All(tg => tg.MandarinTile.PiecesContainer.Count <= 0);
            if (allMandarinTilesEmpty)
            {
                GameOver();
                return;
            }

            ChangePlayer();

            var tileGroup = _board.Sides[CurrentPlayer.Index];
            var isNewPlayerAllEmpty = tileGroup.CitizenTiles.All(t => t.PiecesContainer.Count <= 0);
            if (isNewPlayerAllEmpty)
            {
                if (CurrentPlayer.PieceBench.Pieces.Count <= 0)
                {
                    GameOver();
                    return;
                }

                //Take back pieces to board
                _dropper.Take(CurrentPlayer.PieceBench.Pieces, tileGroup.CitizenTiles.Length);
                _dropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), true);
                _dropper.DropOnce(_ =>
                {
                    _interact.SetupInteract(_board.Sides[CurrentPlayer.Index], new MoveCommand(this),
                        new MoveCommand(this));
                    _interact.ShowTileChooser();
                });
                return;
            }

            _interact.SetupInteract(_board.Sides[CurrentPlayer.Index], new MoveCommand(this),
                new MoveCommand(this));
            _interact.ShowTileChooser();
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
            for (var i = 0; i < _playersManager.Players.Length; i++)
            {
                foreach (var tile in _board.Sides[i].CitizenTiles)
                {
                    _playersManager.Players[i].PieceBench.Pieces.AddRange(tile.PiecesContainer);
                    tile.PiecesContainer.Clear();
                }

                var sum = _playersManager.Players[i].PieceBench.Pieces.Sum(p => p is Citizen ? 1 : 10);

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