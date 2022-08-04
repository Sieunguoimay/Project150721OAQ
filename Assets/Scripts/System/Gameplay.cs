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
            [SerializeField] private GameObject boardPrefab;
            [SerializeField] private GameObject tileSelector;
            [SerializeField] private PieceManager pieceManager;
            public GameObject BoardPrefab => boardPrefab;
            public GameObject TileSelector => tileSelector;
            public PieceManager PieceManager => pieceManager;
        }

        private readonly Gameplay.GameplaySerializable _gameplaySerializable;

        private Board _board;
        private TileSelector _tileSelector;

        private PlayersManager _playerManager = new ();
        private PieceDropper _pieceDropper = new ();

        private PerMatchData _perMatchData;
        private Player CurrentPlayer => _playerManager.CurrentPlayer;

        public bool IsGameOver { get; private set; }

        public Gameplay(Gameplay.GameplaySerializable gameplaySerializable)
        {
            _gameplaySerializable = gameplaySerializable;
        }

        public void Setup()
        {
            _board = UnityEngine.Object.Instantiate(_gameplaySerializable.BoardPrefab).GetComponent<Board>();
            _board.Setup();

            _gameplaySerializable.PieceManager.SpawnPieces(_board);

            _pieceDropper = new PieceDropper();
            _pieceDropper.Setup(_board);
            _pieceDropper.OnDone -= OnDropperDone;
            _pieceDropper.OnDone += OnDropperDone;
            _pieceDropper.OnEat -= OnEatPieces;
            _pieceDropper.OnEat += OnEatPieces;

            _tileSelector = UnityEngine.Object.Instantiate(_gameplaySerializable.TileSelector).GetComponent<TileSelector>();
            _tileSelector.Setup();

            _playerManager = new PlayersManager();
            _playerManager.Setup(_board.TileGroups, _tileSelector);

            foreach (var player in _playerManager.Players)
            {
                player.OnDecisionResult += OnDecisionResult;
            }
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
            Array.Sort(pieces, (a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });
            var delay = 0f;
            for (var i = 0; i < pieces.Length; i++)
            {
                pieces[i].PieceActivityQueue.Add(new Delay(delay += 0.2f));
                pieces[i].PieceScheduler.JumpingMoveTo(positions[i]);
            }
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
            new GameReset(_board, _playerManager).Reset();
            context.Delay(1f, () => { CurrentPlayer.MakeDecision(_board); });
        }
    }
}