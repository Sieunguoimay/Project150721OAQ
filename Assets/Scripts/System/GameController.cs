using System.Collections.Generic;
using Gameplay;
using Implementations.Transporter;
using InGame;
using InGame.ShippingSystem;
using UnityEngine;

namespace System
{
    public class GameController
    {
        private readonly Main.Config _config;

        private Board _board;
        // private DroneMono _droneMono;
        private TileSelector _tileSelector;

        private PlayersManager _playerManager = new PlayersManager();
        private PieceDropper _pieceDropper = new PieceDropper();

        private PerMatchData _perMatchData;
        private Player CurrentPlayer => _playerManager.CurrentPlayer;

        public bool IsGameOver { get; private set; }

        public GameController(Main.Config config)
        {
            _config = config;
        }

        public void Setup()
        {
            _board = UnityEngine.Object.Instantiate(_config.BoardPrefab).GetComponent<Board>();
            _board.Setup();

            _pieceDropper = new PieceDropper();
            _pieceDropper.Setup(_board);
            _pieceDropper.OnDone -= OnDropperDone;
            _pieceDropper.OnDone += OnDropperDone;
            _pieceDropper.OnEat -= OnEatPieces;
            _pieceDropper.OnEat += OnEatPieces;

            _tileSelector = UnityEngine.Object.Instantiate(_config.TileSelector).GetComponent<TileSelector>();
            _tileSelector.Setup();

            _playerManager = new PlayersManager();
            _playerManager.Setup(_board.TileGroups, _tileSelector);

            foreach (var player in _playerManager.Players)
            {
                player.OnDecisionResult += OnDecisionResult;
            }

            // _droneMono = UnityEngine.Object.Instantiate(_config.DronePrefab).GetComponent<DroneMono>();
        }


        public void StartNewMatch()
        {
            _perMatchData = new PerMatchData(_playerManager.Players.Length);
            CurrentPlayer.MakeDecision(_board);
        }

        private void OnDecisionResult(Tile tile, bool forward)
        {
            _pieceDropper.GetReady(tile);
            _pieceDropper.DropAll(forward);
        }

        private void OnDropperDone(PieceDropper.ActionID actionID)
        {
            MakeDecision(actionID == PieceDropper.ActionID.DroppingInTurn);
        }

        private void MakeDecision(bool changePlayer)
        {
            var gameOver = true;
            if (!_board.AreMandarinTilesAllEmpty())
            {
                if (changePlayer)
                {
                    _playerManager.ChangePlayer();
                }

                if (Board.IsTileGroupEmpty(CurrentPlayer.TileGroup))
                {
                    if (CurrentPlayer.PieceBench.Pieces.Count > 0)
                    {
                        if (!CurrentPlayer.TileGroup.TakeBackTiles(CurrentPlayer.PieceBench.Pieces, _pieceDropper))
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

        private void OnEatPieces(IPieceHolder pieceContainerMb)
        {
            var bench = CurrentPlayer.PieceBench;
            bench.Grasp(pieceContainerMb, p =>
            {
                var movePos = bench.GetPosAndRot(bench.Pieces.Count - 1).Position;
                p.JumpingMoveTo(movePos);
                // if (p is Mandarin mandarin)
                // {
                //     var pos = bench.GetMandarinPosAndRot(bench.MandarinCount - 1);
                //     mandarin.Passenger.SetTicket(new TransportTicket(new TransportTicket.ConfigData()
                //     {
                //         attachPoint = Vector3.up,
                //         destination = pos.Position
                //     }));
                //     _droneMono.Target.Attach(mandarin.Passenger);
                // }
                // else if (p is Citizen c)
                // {
                //     var movePos = bench.GetPosAndRot(bench.Pieces.Count - bench.MandarinCount - 1).Position;
                //     c.JumpingMoveTo(movePos);
                // }
            });
        }

        private void GameOver()
        {
            CheckForWinner();
            if (!IsGameOver)
            {
                IsGameOver = true;
            }
        }

        private void CheckForWinner()
        {
            for (var i = 0; i < _playerManager.Players.Length; i++)
            {
                foreach (var tile in _board.TileGroups[i].Tiles)
                {
                    _playerManager.Players[i].PieceBench.Grasp(tile);
                }

                var sum = 0;

                foreach (var p in _playerManager.Players[i].PieceBench.Pieces)
                {
                    switch (p)
                    {
                        case Citizen _:
                        case Mandarin _:
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

        public void ResetGame(MonoBehaviour context)
        {
            IsGameOver = false;
            new GameReset(_board, _playerManager).Reset();
            context.Delay(1f, () => { CurrentPlayer.MakeDecision(_board); });
        }
    }
}