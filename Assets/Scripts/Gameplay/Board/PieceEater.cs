using System;
using System.Collections.Generic;
using Common;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public class PieceEater
    {
        private PieceBench _bench;
        private IReadOnlyList<ITile> _tileSpace;
        private Coroutine _coroutine;
        private bool _forward;

        public bool TryEat(IReadOnlyList<ITile> tiles, PieceBench bench, int index, bool forward, Action done)
        {
            _tileSpace = tiles;
            var nextTile = Board.GetSuccessTile(_tileSpace, index, forward);

            if (!IsTileEatable(nextTile, _tileSpace, forward)) return false;

            SetUpForEating(bench, forward, done);
            EatRecursively(nextTile, done);

            return true;
        }

        private void SetUpForEating(PieceBench bench, bool forward, Action done)
        {
            _bench = bench;
            _forward = forward;
        }

        public void Cleanup()
        {
            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }
        }

        private static bool IsTileEatable(ITile tile, IReadOnlyList<ITile> tiles, bool forward)
        {
            return tile.HeldPieces.Count == 0
                   && tile is not IMandarinTile
                   && Board.GetSuccessTile(tiles, tile.TileIndex, forward).HeldPieces.Count > 0;
        }

        private void EatRecursively(ITile tile, Action done)
        {
            if (IsTileEatable(tile, _tileSpace, _forward))
            {
                var successTile = Board.GetSuccessTile(_tileSpace, tile.TileIndex, _forward);

                EatCitizens(successTile);
                if (successTile is IMandarinTile mt) EatMandarin(mt);

                _coroutine = PublicExecutor.Instance.Delay(0.2f, () =>
                {
                    _coroutine = null;
                    EatRecursively(Board.GetSuccessTile(_tileSpace, successTile.TileIndex, _forward), done);
                });
            }
            else
            {
                done?.Invoke();
            }
        }

        private void EatMandarin(IMandarinTile mandarinTile)
        {
            if (mandarinTile.Mandarin == null) return;

            _bench.AddPiece(mandarinTile.Mandarin);
            _bench.GetPosAndRot(_bench.HeldPieces.Count, out var pos, out var rot);
            mandarinTile.Mandarin.Transform.position = pos;
            mandarinTile.SetMandarin(null);
        }

        private void EatCitizens(IPieceContainer pieceContainer)
        {
            var pieces = pieceContainer.HeldPieces;
            var n = pieces.Count;

            var positions = new Vector3[n];
            var centerPoint = Vector3.zero;
            for (var i = 0; i < n; i++)
            {
                _bench.GetPosAndRot(_bench.HeldPieces.Count + i, out var pos, out var rot);
                positions[i] = pos;
                centerPoint += pos;
                _bench.AddPiece(pieces[i]);
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
    }
}