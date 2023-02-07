using System;
using Common;
using Gameplay.Piece;
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

        public static bool IsTileEatable(ITile tile, Board board, bool forward)
        {
            return tile.HeldPieces.Count == 0
                   && tile is not IMandarinTile
                   && Board.GetSuccessTile(board.Tiles, tile, forward).HeldPieces.Count > 0;
        }

        public void EatRecursively(ITile tile)
        {
            if (IsTileEatable(tile, _board, _forward))
            {
                var successTile = Board.GetSuccessTile(_board.Tiles, tile, _forward);

                EatCitizens(successTile);
                if (successTile is IMandarinTile mt) EatMandarin(mt);

                _coroutine = PublicExecutor.Instance.Delay(0.2f, () =>
                {
                    _coroutine = null;
                    EatRecursively(Board.GetSuccessTile(_board.Tiles, successTile, _forward));
                });
            }
            else
            {
                _done?.Invoke();
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
                    citizen.CitizenMove.StraightMove(positions[i], i * 0.2f);
                }
            }

            pieceContainer.Clear();
        }
    }
}