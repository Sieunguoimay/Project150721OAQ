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
    public interface IMoveMaker
    {
        void Grasp(Action doneHandler);
        void Drop(Action doneHandler);
        void Slam(Action doneHandler);
        void Eat(Action doneHandler);
        bool IsValidGrasp();
        bool CanDrop();
        bool HasReachDeadEnd();
        bool IsGraspable();
        bool IsEatable();
    }

    public class MoveMaker : IMoveMaker
    {
        private readonly Board _board;
        private readonly float _singleMoveDuration;

        public class MoveConfig
        {
            public int StartingTileIndex;
            public bool Direction;
            public PieceBench Bench;
        }

        private MoveConfig _moveConfig;
        private int _currentTileIndex;
        private List<Citizen> _graspedCitizens;

        public MoveMaker(Board board, float singleMoveDuration)
        {
            _board = board;
            _singleMoveDuration = singleMoveDuration;
        }

        public void Initialize(MoveConfig moveConfig)
        {
            _moveConfig = moveConfig;
            _currentTileIndex = _moveConfig.StartingTileIndex;
        }

        public void Grasp(Action doneHandler)
        {
            var tile = _board.GetTileAtIndex(_currentTileIndex);
            var nextTile = GetNextTile(_currentTileIndex);

            var tileGrasper = new GroupFacingAnim(tile, nextTile);
            tileGrasper.FaceTowardNextTile();

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                ClaimAllPiecesOwnership(tile);
                doneHandler?.Invoke();
            });
        }

        private ITile GetNextTile(int currentTileIndex)
        {
            var nextTileIndex = BoardTraveller.MoveNext(currentTileIndex, _board.Tiles.Count, _moveConfig.Direction);
            return _board.GetTileAtIndex(nextTileIndex);
        }

        private void ClaimAllPiecesOwnership(IPieceContainer tile)
        {
            _graspedCitizens = tile.HeldPieces.Select(p => p as Citizen).ToList();
            tile.Clear();
        }

        public void Drop(Action doneHandler)
        {
            var nextTile = GetNextTile(_currentTileIndex);

            for (var i = 0; i < _graspedCitizens.Count; i++)
            {
                var target = nextTile.GetPositionAtGridCellIndex(nextTile.GetPiecesCount() + i);
                _graspedCitizens[i].JumpTo(target, null);
            }

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                DeliverFirstPieceOwnership(nextTile);

                _currentTileIndex = nextTile.TileIndex;
                doneHandler?.Invoke();
            });
        }

        private void DeliverFirstPieceOwnership(IPieceContainer tile)
        {
            tile.AddPiece(_graspedCitizens[0]);
            _graspedCitizens.RemoveAt(0);
        }


        public void Slam(Action doneHandler)
        {
            PublicExecutor.Instance.Delay(_singleMoveDuration, () => { doneHandler?.Invoke(); });
        }

        public void Eat(Action doneHandler)
        {
            var tileToEat = GetNextTile(_currentTileIndex);

            EatCitizensAtTile(tileToEat);

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                _currentTileIndex = tileToEat.TileIndex;
                doneHandler?.Invoke();
            });
        }

        private void EatCitizensAtTile(IPieceContainer pieceContainer)
        {
            var n = pieceContainer.HeldPieces.Count;
            var positions = GetNextPositionsInBench(n);
            var centerPoint = GetCenterPoint(positions);
            SortByDistanceToPoint(pieceContainer, centerPoint);
            MoveAllPiecesToCorrespondingPositions(pieceContainer, positions);
            IPieceContainer.TransferAllPiecesOwnership(pieceContainer, _moveConfig.Bench);
        } 

        private Vector3[] GetNextPositionsInBench(int n)
        {
            var positions = new Vector3[n];
            for (var i = 0; i < n; i++)
            {
                _moveConfig.Bench.GetPosAndRot(_moveConfig.Bench.HeldPieces.Count, out var pos, out var rot);
                positions[i] = pos;
            }

            return positions;
        }

        private static Vector3 GetCenterPoint(IReadOnlyCollection<Vector3> positions)
        {
            var sum = positions.Aggregate(Vector3.zero, (current, pos) => current + pos);
            return sum / positions.Count;
        }

        private static void SortByDistanceToPoint(IPieceContainer container, Vector3 centerPoint)
        {
            container.Sort((a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.Transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.Transform.position);
                return da < db ? -1 : 1;
            });
        }

        private static void MoveAllPiecesToCorrespondingPositions(IPieceContainer container, IReadOnlyList<Vector3> positions)
        {
            var pieces = container.HeldPieces;
            for (var i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] is ICitizen citizen)
                {
                    citizen.CitizenMove.StraightMove(positions[i], null, i * 0.2f);
                }
            }
        }

        public bool IsValidGrasp()
        {
            return _board.Tiles[_currentTileIndex].GetPiecesCount() > 0;
        }

        public bool HasReachDeadEnd()
        {
            var nextTile = GetNextTile(_currentTileIndex);
            var nextTile2 = GetNextTile(nextTile.TileIndex);
            return nextTile.GetPiecesCount() == 0 && nextTile2.GetPiecesCount() == 0 || nextTile is IMandarinTile;
        }

        public bool IsEatable()
        {
            var nextTile = GetNextTile(_currentTileIndex);
            var nextTile2 = GetNextTile(nextTile.TileIndex);
            return nextTile.GetPiecesCount() == 0 && nextTile2.GetPiecesCount() > 0;
        }

        public bool IsGraspable()
        {
            var nextTile = GetNextTile(_currentTileIndex);
            return nextTile.GetPiecesCount() > 0 && nextTile is not IMandarinTile;
        }

        public bool CanDrop()
        {
            return _graspedCitizens.Count > 0;
        }
    }

    public class GroupFacingAnim
    {
        private readonly ITile _tile;
        private readonly ITile _nextTile;

        public GroupFacingAnim(ITile tile, ITile nextTile)
        {
            _tile = tile;
            _nextTile = nextTile;
        }

        public void FaceTowardNextTile()
        {
            foreach (var p in _tile.HeldPieces)
            {
                if (p is Citizen ci)
                {
                    ci.StandUpAndRotateTo(_nextTile.GetPosition(), null);
                }
            }
        }
    }
}