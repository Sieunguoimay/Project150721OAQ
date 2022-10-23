using System.Collections.Generic;
using System.Linq;
using Common;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using SNM;
using UnityEngine;

namespace System
{
    public class Gameplay
    {
        private PlayersManager _playersManager;
        private Board _board;
        private PieceManager _pieceManager;
        private PieceDropper _pieceDropper;

        private PerMatchData _perMatchData;
        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }
        private Coroutine _coroutine;

        public void Setup(PlayersManager playersManager, Board board, PieceManager pieceManager)
        {
            _board = board;
            _playersManager = playersManager;
            _pieceManager = pieceManager;

            _pieceDropper = new PieceDropper(_board);

            IsPlaying = false;
            IsGameOver = false;
        }

        public void TearDown()
        {
            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }
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
            _pieceDropper.ClearHoldingPieces();
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

        private void OnDecisionResult(Tile tile, bool forward)
        {
            _pieceDropper.Take(tile.Pieces, tile.Pieces.Count);
            _pieceDropper.SetMoveStartPoint(Array.IndexOf(_board.Tiles, tile), forward);
            _pieceDropper.DropNonStop(lastTile =>
                EatRecursively(_board.GetSuccessTile(lastTile, forward), forward, () => { MakeDecision(true); }));
        }

        private void EatRecursively(Tile tile, bool forward, Action done)
        {
            if (CheckSuccessEatable(tile, forward))
            {
                var successTile = _board.GetSuccessTile(tile, forward);

                EatPieces(successTile.Pieces);

                _coroutine = PublicExecutor.Instance.Delay(0.2f, () =>
                {
                    _coroutine = null;
                    EatRecursively(_board.GetSuccessTile(successTile, forward), forward, done);
                });
            }
            else
            {
                done?.Invoke();
            }
        }

        private bool CheckSuccessEatable(Tile t, bool f)
        {
            return t.Pieces.Count == 0 && t is not MandarinTile && _board.GetSuccessTile(t, f).Pieces.Count > 0;
        }

        private void MakeDecision(bool changePlayer)
        {
            bool gameOver;
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
                        _pieceDropper.DropOnce(_ => { MakeDecision(false); });
                        gameOver = false;
                    }
                    else
                    {
                        gameOver = true;
                    }
                }
                else
                {
                    CurrentPlayer.MakeDecision(_board, OnDecisionResult);
                    gameOver = false;
                }
            }
            else
            {
                gameOver = true;
            }

            if (gameOver)
            {
                GameOver();
            }
        }

        private void EatPieces(List<Piece> pieces)
        {
            var bench = CurrentPlayer.PieceBench;
            var n = pieces.Count;

            var positions = new Vector3[n];
            var centerPoint = Vector3.zero;
            var startIndex = bench.Pieces.Count;
            for (var i = 0; i < n; i++)
            {
                positions[i] = bench.GetPosAndRot(startIndex + i).Position;
                centerPoint += positions[i];
                bench.Pieces.Add(pieces[i]);
            }

            centerPoint /= n;

            pieces.Sort((a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });

            for (var i = 0; i < pieces.Count; i++)
            {
                pieces[i].ActivityQueue.Add(i > 0 ? new ActivityDelay(i * 0.2f) : null);
                pieces[i].ActivityQueue.Add(pieces[i].Animator
                    ? new ActivityAnimation(pieces[i].Animator, LegHashes.stand_up)
                    : null);
                pieces[i].ActivityQueue.Add(new ActivityFlocking(pieces[i].FlockingConfigData, positions[i],
                    pieces[i].transform, null));
                pieces[i].ActivityQueue.Add(pieces[i].Animator
                    ? new ActivityAnimation(pieces[i].Animator, LegHashes.sit_down)
                    : null);
                pieces[i].ActivityQueue.Begin();
            }

            pieces.Clear();
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