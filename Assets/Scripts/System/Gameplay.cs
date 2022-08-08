using System.Collections.Generic;
using CommonActivities;
using Gameplay;
using InGame;
using SNM;
using UnityEngine;

namespace System
{
    public class Gameplay
    {
        [Serializable]
        public class GameplaySerializable
        {
            public Board boardPrefab;
            public TileSelector tileSelectorPrefab;
            public PieceManager pieceManager;
            public PlayersManager playersManager;
        }

        private readonly Gameplay.GameplaySerializable _gameplaySerializable;
        private PlayersManager PlayerManager => _gameplaySerializable.playersManager;
        public Board BoardPrefab => _gameplaySerializable.boardPrefab;
        public TileSelector TileSelectorPrefabPrefab => _gameplaySerializable.tileSelectorPrefab;
        public PieceManager PieceManager => _gameplaySerializable.pieceManager;
        
        private Board _board;
        private TileSelector _tileSelector;

        private PieceDropper _pieceDropper = new();

        private PerMatchData _perMatchData;
        private Player CurrentPlayer => PlayerManager.CurrentPlayer;

        public bool IsGameOver { get; private set; }
        public bool IsPlaying { get; private set; }

        public Gameplay(Gameplay.GameplaySerializable gameplaySerializable)
        {
            _gameplaySerializable = gameplaySerializable;
        }

        public void Setup()
        {
            _tileSelector = UnityEngine.Object.Instantiate(TileSelectorPrefabPrefab);
            _tileSelector.Setup();

            _board = UnityEngine.Object.Instantiate(BoardPrefab);
            _board.Setup();

            PlayerManager.Setup(_board.TileGroups, _tileSelector);

            PieceManager.SpawnPieces(PlayerManager.Players);

            _pieceDropper = new PieceDropper();
            _pieceDropper.Setup(_board);


            ConnectEvents();
        }

        public void TearDown()
        {
            DisconnectEvents();
        }

        private void ConnectEvents()
        {
            _pieceDropper.OnDone += OnDropperDone;
            _pieceDropper.OnEat += OnEatPieces;
            foreach (var player in PlayerManager.Players)
            {
                player.OnDecisionResult += OnDecisionResult;
            }
        }

        private void DisconnectEvents()
        {
            _pieceDropper.OnDone -= OnDropperDone;
            _pieceDropper.OnEat -= OnEatPieces;
            foreach (var player in PlayerManager.Players)
            {
                player.OnDecisionResult -= OnDecisionResult;
            }
        }

        public void StartNewMatch()
        {
            IsPlaying = true;
            _perMatchData = new PerMatchData(PlayerManager.Players.Length);
            PieceManager.ReleasePieces(() => { CurrentPlayer.MakeDecision(_board); });
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
                    PlayerManager.ChangePlayer();
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
            for (var i = 0; i < PlayerManager.Players.Length; i++)
            {
                foreach (var tile in _board.TileGroups[i].Tiles)
                {
                    PlayerManager.Players[i].PieceBench.Grasp(tile);
                }

                var sum = 0;

                foreach (var p in PlayerManager.Players[i].PieceBench.Pieces)
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

        public void ResetGame(MonoBehaviour context)
        {
            IsGameOver = false;
            new GameReset(_board, PlayerManager).Reset();
            context.Delay(1f, () => { CurrentPlayer.MakeDecision(_board); });
        }
    }
}