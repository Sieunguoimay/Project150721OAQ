﻿using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using UnityEngine;

namespace System
{
    public class Gameplay
    {
        private PlayersManager _playersManager;
        private Board _board;
        private PieceManager _pieceManager;

        private readonly PieceDropper _pieceDropper = new();

        private PerMatchData _perMatchData;
        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }

        public void Setup(PlayersManager playersManager, Board board, PieceManager pieceManager)
        {
            _board = board;
            _playersManager = playersManager;
            _pieceManager = pieceManager;

            _pieceDropper.Setup(_board);

            ConnectEvents();

            IsPlaying = false;
            IsGameOver = false;
        }

        public void TearDown()
        {
            DisconnectEvents();
        }

        public void StartNewMatch()
        {
            IsPlaying = true;
            ChangePlayer();
            _perMatchData = new PerMatchData(_playersManager.Players.Length);
            _pieceManager.ReleasePieces(() => { CurrentPlayer.MakeDecision(_board, OnDecisionResult); }, _board);
        }

        public void ResetGame()
        {
            IsGameOver = false;
            IsPlaying = false;
            _pieceDropper.Reset();
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

        private void ConnectEvents()
        {
            _pieceDropper.OnEat += OnEatPieces;
        }

        private void DisconnectEvents()
        {
            _pieceDropper.OnEat -= OnEatPieces;
        }

        private void OnDecisionResult(Tile tile, bool forward)
        {
            _pieceDropper.Take(tile.Pieces, tile.Pieces.Count);
            _pieceDropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tile), forward);
            _pieceDropper.DropAll(OnDropAllDone);
        }

        private void OnDropAllDone()
        {
            _pieceDropper.Continue(() => { MakeDecision(true); });
        }


        private void MakeDecision(bool changePlayer)
        {
            var gameOver = true;
            var allMandarinTilesEmpty = _board.TileGroups.All(tg => tg.MandarinTile.Pieces.Count <= 0);
            if (!allMandarinTilesEmpty)
            {
                if (changePlayer)
                {
                    ChangePlayer();
                }

                var tileGroup = _board.TileGroups[CurrentPlayer.Index];
                var isNewPlayerAllEmpty = tileGroup.Tiles.All(t => t.Pieces.Count <= 0);
                if (isNewPlayerAllEmpty)
                {
                    if (CurrentPlayer.PieceBench.Pieces.Count > 0)
                    {
                        //Take back pieces to board
                        _pieceDropper.Take(CurrentPlayer.PieceBench.Pieces, tileGroup.Tiles.Length);
                        _pieceDropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), true);
                        _pieceDropper.DropAll(() => { MakeDecision(false); });
                        gameOver = false;
                    }
                }
                else
                {
                    CurrentPlayer.MakeDecision(_board, OnDecisionResult);
                    gameOver = false;
                }
            }

            if (gameOver)
            {
                GameOver();
            }
        }

        private void OnEatPieces(Tile pieceContainerMb)
        {
            var n = pieceContainerMb.Pieces.Count;

            var bench = CurrentPlayer.PieceBench;
            var positions = new Vector3[n];
            var pieces = new Piece[n];
            var centerPoint = Vector3.zero;
            var startIndex = bench.Pieces.Count;
            for (var i = 0; i < n; i++)
            {
                positions[i] = bench.GetPosAndRot(startIndex + i).Position;
                var p = pieceContainerMb.Pieces[i];
                pieces[i] = p;
                centerPoint += positions[i];
                bench.Pieces.Add(p);
            }

            pieceContainerMb.Pieces.Clear();

            centerPoint /= n;

            PieceScheduler.MovePiecesOutOfTheBoard(pieces, positions, centerPoint);
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
                foreach (var tile in _board.TileGroups[i].Tiles)
                {
                    _playersManager.Players[i].PieceBench.Pieces.AddRange(tile.Pieces);
                    tile.Pieces.Clear();
                }

                var sum = 0;

                foreach (var p in _playersManager.Players[i].PieceBench.Pieces)
                {
                    switch (p)
                    {
                        case Citizen:
                            sum += 1;
                            break;
                        case Mandarin:
                            sum += 10;
                            break;
                    }
                }

                _perMatchData.SetPlayerScore(i, sum);
            }

            TellWinner(_perMatchData.PlayerScores);
        }

        private static void TellWinner(IReadOnlyList<int> scores)
        {
            if (scores[0] > scores[1])
            {
                Debug.Log("Player 1 win! " + scores[0] + " - " + scores[1]);
            }
            else if (scores[0] < scores[1])
            {
                Debug.Log("Player 2 win! " + scores[0] + " - " + scores[1]);
            }
            else
            {
                Debug.Log("Draw! " + scores[0] + " - " + scores[1]);
            }
        }

        #endregion
    }
}