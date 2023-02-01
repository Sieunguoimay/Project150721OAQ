using System.Collections.Generic;
using System.Linq;
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

            _dropper.SetBoard(_board);
            _eater.SetBoard(_board);

            IsPlaying = false;
            IsGameOver = false;
            NextPlayer();
        }

        public void StartNewMatch()
        {
            IsPlaying = true;
            _interact.SetupInteract(_board.Sides[CurrentPlayer.Index], new MoveCommand(this), new MoveCommand(this));
            _interact.ShowTileChooser();
        }

        public void ClearGame()
        {
            IsGameOver = false;
            IsPlaying = false;
            (_dropper as IPieceContainer)?.Clear();
            _eater.Cleanup();
            CurrentPlayer = null;
            _interact.ResetAll();
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


        private class MoveCommand : GameInteractManager.MoveButtonCommand
        {
            private readonly Gameplay _gameplay;

            public MoveCommand(Gameplay gameplay)
            {
                _gameplay = gameplay;
            }

            protected override void Move(ITile tile, bool forward)
            {
                _gameplay._dropper.Take(tile, tile.HeldPieces.Count);
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
                _dropper.Take(CurrentPlayer.PieceBench, tileGroup.CitizenTiles.Length);
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