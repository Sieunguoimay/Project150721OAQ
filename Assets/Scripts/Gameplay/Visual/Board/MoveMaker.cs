using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Helpers;
using Gameplay.Player;
using Gameplay.Visual.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class MoveMaker : IMoveMaker, MoveInnerRules<Tile>.IMoveRuleDataHelper
    {
        private readonly Board _board;
        private readonly float _singleMoveDuration;
        private List<Citizen> _graspedCitizens;
        private readonly GridLocator _gridLocator;

        public class MoveConfig
        {
            public int StartingTileIndex;
            public bool Direction;
            public PieceBench Bench;
        }

        private MoveConfig _moveConfig;

        public MoveMaker(Board board, float singleMoveDuration, GridLocator gridLocator)
        {
            _board = board;
            _singleMoveDuration = singleMoveDuration;
            _gridLocator = gridLocator;
            MoveInnerRules = new MoveInnerRules<Tile>(this);
        }

        public void Initialize(MoveConfig moveConfig)
        {
            _moveConfig = moveConfig;
            TileIterator = new TileIterator<Tile>(_board.Tiles, moveConfig.Direction);
            TileIterator.UpdateCurrentTileIndex(_moveConfig.StartingTileIndex);
        }

        public void Grasp(Action doneHandler)
        {
            var tileGrasper = new GroupFacingAnim(TileIterator.CurrentTile, TileIterator.NextTile);
            tileGrasper.FaceTowardNextTile();

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                ClaimAllPiecesOwnership(TileIterator.CurrentTile);
                TileIterator.UpdateCurrentTileIndex(TileIterator.NextTile.TileIndex);
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
                var gridIndex = TileIterator.CurrentTile.GetNumTakenGridCells() + i;
                var target = _gridLocator.GetPositionAtCellIndex(TileIterator.CurrentTile.transform, gridIndex);
                _graspedCitizens[i].Animator.JumpTo(target, null);
            }

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                DeliverFirstPieceOwnership(TileIterator.CurrentTile);

                TileIterator.UpdateCurrentTileIndex(TileIterator.NextTile.TileIndex);
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
            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                TileIterator.UpdateCurrentTileIndex(TileIterator.NextTile.TileIndex);
                doneHandler?.Invoke();
            });
        }

        public void Eat(Action doneHandler)
        {
            EatCitizensAtTile(TileIterator.CurrentTile);

            PublicExecutor.Instance.Delay(_singleMoveDuration, () =>
            {
                TileIterator.UpdateCurrentTileIndex(TileIterator.NextTile.TileIndex);
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
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });
        }

        private static void MoveAllPiecesToCorrespondingPositions(IPieceContainer container,
            IReadOnlyList<Vector3> positions)
        {
            var pieces = container.HeldPieces;
            for (var i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] is Citizen citizen)
                {
                    citizen.Animator.StraightMove(positions[i], null, i * 0.2f);
                }
            }
        }

        public bool CanDrop()
        {
            return _graspedCitizens.Count > 0;
        }

        public IMoveInnerRules MoveInnerRules { get; }
        public TileIterator<Tile> TileIterator { get; private set; }

        public int GetNumPiecesInTile(Tile tile)
        {
            return tile.GetPiecesCount();
        }

        public bool IsMandarinTile(Tile tile)
        {
            return tile is MandarinTile;
        }
    }

    public class GroupFacingAnim
    {
        private readonly Tile _tile;
        private readonly Tile _nextTile;

        public GroupFacingAnim(Tile tile, Tile nextTile)
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
                    ci.Animator.StandUpAndRotateTo(_nextTile.GetPosition(), null);
                }
            }
        }
    }
}