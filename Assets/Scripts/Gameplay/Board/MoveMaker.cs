using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.DecisionMaking;
using Gameplay.Piece;
using Gameplay.Player;
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
        private ITile _currentTile;
        private ITile _nextTile;
        private ITile _nextTile2;
        private List<Citizen> _graspedCitizens;

        public MoveMaker(Board board, float singleMoveDuration)
        {
            _board = board;
            _singleMoveDuration = singleMoveDuration;
        }

        public void Initialize(MoveConfig moveConfig)
        {
            _moveConfig = moveConfig;
            UpdateCurrentTileIndex(_moveConfig.StartingTileIndex);
        }

        public void Grasp(Action doneHandler)
        {

            var tileGrasper = new GroupFacingAnim(_currentTile, _nextTile);
            tileGrasper.FaceTowardNextTile();

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                ClaimAllPiecesOwnership(_currentTile);
                doneHandler?.Invoke();
            });
        }

        private void ClaimAllPiecesOwnership(IPieceContainer tile)
        {
            _graspedCitizens = tile.HeldPieces.Select(p => p as Citizen).ToList();
            tile.Clear();
        }

        public void Drop(Action doneHandler)
        {

            for (var i = 0; i < _graspedCitizens.Count; i++)
            {
                var target = _nextTile.GetPositionAtGridCellIndex(_nextTile.GetPiecesCount() + i);
                _graspedCitizens[i].JumpTo(target, null);
            }

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                DeliverFirstPieceOwnership(_nextTile);

                UpdateCurrentTileIndex(_nextTile.TileIndex);
                
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
            EatCitizensAtTile(_nextTile);

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                UpdateCurrentTileIndex(_nextTile.TileIndex);
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

        private static void MoveAllPiecesToCorrespondingPositions(IPieceContainer container,
            IReadOnlyList<Vector3> positions)
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
            return _currentTile.GetPiecesCount() > 0;
        }

        public bool HasReachDeadEnd()
        {
            return _nextTile.GetPiecesCount() == 0 && _nextTile2.GetPiecesCount() == 0 || _nextTile is IMandarinTile;
        }

        public bool IsEatable()
        {
            return _nextTile.GetPiecesCount() == 0 && _nextTile2.GetPiecesCount() > 0;
        }

        public bool IsGraspable()
        {
            return _nextTile.GetPiecesCount() > 0 && _nextTile is not IMandarinTile;
        }

        public bool CanDrop()
        {
            return _graspedCitizens.Count > 0;
        }

        private void UpdateCurrentTileIndex(int currentTileIndex)
        {
            _currentTile = _board.GetTileAtIndex(currentTileIndex);
            _nextTile = GetNextTile(_currentTile.TileIndex);
            _nextTile2 = GetNextTile(_nextTile.TileIndex);
        }
        
        private ITile GetNextTile(int currentTileIndex)
        {
            var nextTileIndex = BoardTraveller.MoveNext(currentTileIndex, _board.Tiles.Count, _moveConfig.Direction);
            return _board.GetTileAtIndex(nextTileIndex);
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