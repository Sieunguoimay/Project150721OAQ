using System;
using System.Collections.Generic;
using Common;
using Common.Activity;
using Gameplay.Piece.Activities;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public class PieceEater
    {
        private PieceBench _bench;
        private Board _board;
        private Coroutine _coroutine;
        private bool _forward;
        private Action _done;

        public void SetBoard(Board board)
        {
            _board = board;
        }

        public void SetUpForEating(PieceBench bench, bool forward, Action done)
        {
            _bench = bench;
            _forward = forward;
            _done = done;
        }

        public void Cleanup()
        {
            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }
        }

        public void EatRecursively(Tile tile)
        {
            var eatable = tile.Pieces.Count == 0 && tile is not MandarinTile &&
                          _board.GetSuccessTile(tile, _forward).Pieces.Count > 0;
            if (eatable)
            {
                var successTile = _board.GetSuccessTile(tile, _forward);

                EatPieces(successTile.Pieces);

                _coroutine = PublicExecutor.Instance.Delay(0.2f, () =>
                {
                    _coroutine = null;
                    EatRecursively(_board.GetSuccessTile(successTile, _forward));
                });
            }
            else
            {
                _done?.Invoke();
            }
        }

        private void EatPieces(List<Piece.Piece> pieces)
        {
            var n = pieces.Count;

            var positions = new Vector3[n];
            var centerPoint = Vector3.zero;
            var startIndex = _bench.Pieces.Count;
            for (var i = 0; i < n; i++)
            {
                _bench.GetPosAndRot(startIndex + i, out var pos, out var rot);
                positions[i] = pos;
                centerPoint += positions[i];
                _bench.Pieces.Add(pieces[i]);
            }

            centerPoint /= n;

            pieces.Sort((a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });

            for (var i = 0; i < pieces.Count; i++)
            {
                pieces[i].ActivityQueue.Add(i > 0 ? new ActivityDelay(i * 0.2f) : null);
                pieces[i].ActivityQueue.Add(pieces[i].Animator
                    ? new ActivityAnimation(pieces[i].Animator, LegHashes.stand_up)
                    : null);
                pieces[i].ActivityQueue.Add(new ActivityFlocking(pieces[i].FlockingConfigData, positions[i],
                    pieces[i].transform, null));
                pieces[i].ActivityQueue.Add(pieces[i].Animator
                    ? new ActivityAnimation(pieces[i].Animator, LegHashes.sit_down)
                    : null);
                pieces[i].ActivityQueue.Begin();
            }

            pieces.Clear();
        }
    }
}