using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.DecisionMaking;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IBoardActionExecutor
    {
        void Grasp(Action doneHandler);
        void Drop(Action doneHandler);
        void Slam(Action doneHandler);
        void Eat(Action doneHandler);
        bool IsValidGrasp();
        bool CanDrop();
        bool CanEatMore();
        IState GetSuccessStateAfterDrop(IState idle, IState slamAndEat, IState grasp);
    }

    public class BoardActionExecutor : IBoardActionExecutor
    {
        public class Argument
        {
            public Board board;
            public int tileIndex;
            public bool direction;
            public float singleActionDuration;
            public PieceBench bench;
        }

        private Argument _argument;
        private int _currentTileIndex;
        private List<Citizen> _graspedCitizens;

        public void Initialize(Argument argument)
        {
            _argument = argument;
            _currentTileIndex = _argument.tileIndex;
        }

        public void Grasp(Action doneHandler)
        {
            var tile = _argument.board.Tiles[_currentTileIndex];
            var nextTileIndex =
                BoardTraveller.MoveNext(_currentTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTile = _argument.board.Tiles[nextTileIndex];

            foreach (var p in tile.HeldPieces)
            {
                if (p is Citizen ci)
                {
                    ci.StandUpAndRotateTo(nextTile.Transform.position, null);
                }
            }

            PublicExecutor.Instance.Delay(_argument.singleActionDuration, () =>
            {
                _graspedCitizens = tile.HeldPieces.Select(p => p as Citizen).ToList();
                tile.Clear();
                doneHandler?.Invoke();
            });
        }

        public void Drop(Action doneHandler)
        {
            var nextTileIndex =
                BoardTraveller.MoveNext(_currentTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTile = _argument.board.Tiles[nextTileIndex];
            for (var i = 0; i < _graspedCitizens.Count; i++)
            {
                var ci = _graspedCitizens[i];
                var target = nextTile.GetGridPosition(nextTile.HeldPieces.Count + i);
                ci.JumpTo(target, null);
            }

            PublicExecutor.Instance.Delay(_argument.singleActionDuration, () =>
            {
                nextTile.AddPiece(_graspedCitizens[0]);
                _graspedCitizens.RemoveAt(0);
                _currentTileIndex = nextTileIndex;
                doneHandler?.Invoke();
            });
        }

        public void Slam(Action doneHandler)
        {
            PublicExecutor.Instance.Delay(_argument.singleActionDuration, () => { doneHandler?.Invoke(); });
        }

        public void Eat(Action doneHandler)
        {
            var nextTileIndex =
                BoardTraveller.MoveNext(_currentTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTile = _argument.board.Tiles[nextTileIndex];

            EatCitizensAtTile(nextTile);

            PublicExecutor.Instance.Delay(_argument.singleActionDuration, () =>
            {
                _currentTileIndex = nextTileIndex;
                doneHandler?.Invoke();
            });
        }

        private void EatCitizensAtTile(IPieceContainer pieceContainer)
        {
            var pieces = pieceContainer.HeldPieces;
            var n = pieces.Count;

            var positions = new Vector3[n];
            var centerPoint = Vector3.zero;
            for (var i = 0; i < n; i++)
            {
                _argument.bench.GetPosAndRot(_argument.bench.HeldPieces.Count, out var pos, out var rot);
                positions[i] = pos;
                centerPoint += pos;
                _argument.bench.AddPiece(pieces[i]);
            }

            centerPoint /= n;

            pieceContainer.Sort((a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.Transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.Transform.position);
                return da < db ? -1 : 1;
            });

            for (var i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] is ICitizen citizen)
                {
                    citizen.CitizenMove.StraightMove(positions[i], null, i * 0.2f);
                }
            }

            pieceContainer.Clear();
        }

        public bool IsValidGrasp()
        {
            return _argument.board.Tiles[_currentTileIndex].HeldPieces.Count > 0;
        }

        public bool CanEatMore()
        {
            var nextTileIndex =
                BoardTraveller.MoveNext(_currentTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTileIndex2 =
                BoardTraveller.MoveNext(nextTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTile = _argument.board.Tiles[nextTileIndex];
            var nextTile2 = _argument.board.Tiles[nextTileIndex2];

            return nextTile.HeldPieces.Count == 0 && nextTile2.HeldPieces.Count > 0;
        }

        public IState GetSuccessStateAfterDrop(IState idle, IState slamAndEat, IState grasp)
        {
            var nextTileIndex =
                BoardTraveller.MoveNext(_currentTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTileIndex2 =
                BoardTraveller.MoveNext(nextTileIndex, _argument.board.Tiles.Count, _argument.direction);
            var nextTile = _argument.board.Tiles[nextTileIndex];
            var nextTile2 = _argument.board.Tiles[nextTileIndex2];

            if (nextTile.HeldPieces.Count == 0)
            {
                if (nextTile2.HeldPieces.Count == 0)
                {
                    return idle;
                }

                _currentTileIndex = nextTileIndex;
                return slamAndEat;
            }

            if (nextTile is IMandarinTile)
            {
                return idle;
            }

            _currentTileIndex = nextTileIndex;
            return grasp;
        }

        public bool CanDrop()
        {
            return _graspedCitizens.Count > 0;
        }
    }
}