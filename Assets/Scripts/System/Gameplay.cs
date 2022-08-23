using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using UnityEngine;

namespace System
{
    public class Gameplay
    {
        private Player[] _players;
        private Board _board;
        private PieceManager _pieceManager;
        
        private readonly PieceDropper _pieceDropper = new();

        private PerMatchData _perMatchData;
        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }

        public void Setup(Player[] players, Board board, PieceManager pieceManager)
        {
            _board = board;
            _players = players;
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
            _perMatchData = new PerMatchData(_players.Length);
            _pieceManager.ReleasePieces(() =>
            {
                CurrentPlayer.AcquireTurn();
                CurrentPlayer.MakeDecision(_board);
            }, _board);
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
                CurrentPlayer = _players[0];
            }
            else
            {
                CurrentPlayer.ReleaseTurn();
                CurrentPlayer = _players[(CurrentPlayer.Index + 1) % _players.Length];
            }

            CurrentPlayer.AcquireTurn();
        }

        private void ConnectEvents()
        {
            _pieceDropper.OnDone += OnDropperDone;
            _pieceDropper.OnEat += OnEatPieces;
            foreach (var player in _players)
            {
                player.OnDecisionResult += OnDecisionResult;
            }
        }

        private void DisconnectEvents()
        {
            _pieceDropper.OnDone -= OnDropperDone;
            _pieceDropper.OnEat -= OnEatPieces;
            foreach (var player in _players)
            {
                player.OnDecisionResult -= OnDecisionResult;
            }
        }

        private void OnDecisionResult(Tile tile, bool forward)
        {
            _pieceDropper.Pickup(tile);
            _pieceDropper.DropAll(forward);
        }

        private void OnDropperDone(PieceDropper.ActionID actionID)
        {
            MakeDecision(actionID == PieceDropper.ActionID.DroppingInTurn);
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

                if (_board.TileGroups[CurrentPlayer.Index].Tiles.All(t => t.Pieces.Count <= 0))
                {
                    if (CurrentPlayer.PieceBench.Pieces.Count > 0)
                    {
                        if (!TakeBackCitizens(CurrentPlayer.PieceBench.Pieces, _pieceDropper,
                            _board.TileGroups[CurrentPlayer.Index]))
                        {
                            gameOver = false;
                        }
                    }
                }
                else
                {
                    CurrentPlayer.MakeDecision(_board);
                    gameOver = false;
                }
            }

            if (gameOver)
            {
                GameOver();
            }
        }

        private static bool TakeBackCitizens(List<Piece> pieces, PieceDropper dropper, Board.TileGroup tg)
        {
            if (pieces.Count <= 0) return false;

            dropper.GetReadyForTakingBackCitizens(tg, pieces);
            dropper.DropAll(true);

            return true;
        }

        private void OnEatPieces(IPieceHolder pieceContainerMb)
        {
            var bench = CurrentPlayer.PieceBench;
            var positions = new Vector3[pieceContainerMb.Pieces.Count];
            var pieces = new Piece[pieceContainerMb.Pieces.Count];
            var count = 0;
            var centerPoint = Vector3.zero;

            bench.Grasp(pieceContainerMb, p =>
            {
                positions[count] = bench.GetPosAndRot(bench.Pieces.Count - 1).Position;
                pieces[count] = p;
                centerPoint += positions[count++];
            });

            centerPoint /= count;

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
            for (var i = 0; i < _players.Length; i++)
            {
                foreach (var tile in _board.TileGroups[i].Tiles)
                {
                    _players[i].PieceBench.Grasp(tile);
                }

                var sum = 0;

                foreach (var p in _players[i].PieceBench.Pieces)
                {
                    switch (p)
                    {
                        case Citizen:
                        case Mandarin:
                            sum += p.Config.point;
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