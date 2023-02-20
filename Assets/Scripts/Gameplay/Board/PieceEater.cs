using System;
using System.Collections.Generic;
using Common;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    [Obsolete]
    public class PieceEater
    {
        private IReadOnlyList<ITile> _tileSpace;
        private PieceBench _bench;
        private bool _forward;

        private Coroutine _coroutine;

        public bool TryEat(IReadOnlyList<ITile> tiles, PieceBench bench, int index, bool forward, Action done)
        {
            _tileSpace = tiles;
            _bench = bench;
            _forward = forward;
            
            return CheckToEat(index, done);
        }

        public void Cleanup()
        {
            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }
        }

        private bool IsEatable(int tileIndex1, int tileIndex2)
        {
            return _tileSpace[tileIndex1].HeldPieces.Count == 0
                   && _tileSpace[tileIndex1] is not IMandarinTile
                   && _tileSpace[tileIndex2].HeldPieces.Count > 0;
        }

        private void EatRecursively(int index, Action done)
        {
            EatCitizens(index);
            if (_tileSpace[index] is IMandarinTile mt) EatMandarin(mt);

            _coroutine = PublicExecutor.Instance.Delay(0.2f, () =>
            {
                _coroutine = null;
                if (!CheckToEat(index, done)) done?.Invoke();
            });
        }

        private bool CheckToEat(int index, Action done)
        {
            var nextTile1 = BoardTraveller.MoveNext(index, _tileSpace.Count, _forward);
            var nextTile2 = BoardTraveller.MoveNext(nextTile1, _tileSpace.Count, _forward);
            if (!IsEatable(nextTile1, nextTile2)) return false;
            PublicExecutor.Instance.Delay(.5f, () =>
            {
                EatRecursively(nextTile2, done);
            });
            return true;
        }

        private void EatMandarin(IMandarinTile mandarinTile)
        {
            if (mandarinTile.Mandarin == null) return;

            _bench.AddPiece(mandarinTile.Mandarin);
            _bench.GetPosAndRot(_bench.HeldPieces.Count, out var pos, out var rot);
            mandarinTile.Mandarin.Transform.position = pos;
            mandarinTile.SetMandarin(null);
        }

        private void EatCitizens(int tileIndex)
        {
            var pieceContainer = _tileSpace[tileIndex];
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