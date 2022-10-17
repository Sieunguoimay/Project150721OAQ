using System;
using System.Collections.Generic;
using Common;
using CommonActivities;
using DG.Tweening;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PieceDropper
    {
        private readonly List<Piece.Piece> _pieces = new();

        private readonly BoardTraveller _boardTraveller = new();
        public event Action<Tile> OnEat = delegate { };

        private bool _forward;
        private Board.Board _board;
        private Tile CurrentTile => _board.Tiles[_boardTraveller.CurrentIndex];
        private Coroutine _coroutine;

        public void Setup(Board.Board board)
        {
            _board = board;
        }

        public void Reset()
        {
            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }

            _pieces.Clear();
        }

        public void Take(List<Piece.Piece> pieces, int num)
        {
            ForwardLastItems(pieces, _pieces, num);
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, _pieces.Count, _board.Tiles.Length);
            _forward = forward;
        }

        public static void ForwardLastItems<T>(List<T> source, List<T> target, int num)
        {
            if (num == source.Count)
            {
                target.AddRange(source);
                source.Clear();
                return;
            }

            target.AddRange(source.GetRange(source.Count - num, num));
            source.RemoveRange(source.Count - num, num);
        }

        private class ActivityRotatePiece : Activity
        {
            private readonly Vector3 _citizenPos;
            private readonly Piece.Piece _p;
            private readonly float _duration;

            public ActivityRotatePiece(Vector3 pos, Piece.Piece p, float duration)
            {
                _citizenPos = pos;
                _p = p;
                _duration = duration;
            }

            public override void Begin()
            {
                base.Begin();

                var euler = Quaternion.LookRotation(_citizenPos - _p.transform.position).eulerAngles;
                var targetEuler = _p.transform.eulerAngles;
                targetEuler.y = euler.y;
                _p.transform.DORotate(targetEuler, _duration).SetLink(_p.gameObject).OnComplete(NotifyDone);
            }
        }

        public void DropAll(Action done)
        {
            var delay = 0f;
            var n = _pieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);

                for (var j = 0; j < n - i; j++)
                {
                    if (_pieces[i + j] is not Citizen p) continue;

                    var skipSlot = CurrentTile is MandarinTile;
                    var citizenPos =
                        CurrentTile.GetPositionInFilledCircle(CurrentTile.Pieces.Count + j + (skipSlot ? 5 : 0));

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new ActivityRotatePiece(citizenPos, p, delay));

                        delay += 0.2f;
                    }

                    PieceScheduler.CreateJumpTimelineActivity(p, citizenPos);

                    if (i == n - 1)
                    {
                        p.PieceActivityQueue.Add(new ActivityFinalize(done));
                    }
                }

                CurrentTile.Pieces.Add(_pieces[i]);
            }

            foreach (var p in _pieces)
            {
                p.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(p.transform));
                PieceScheduler.CreateAnimActivity(p, LegHashes.sit_down, null);
                p.PieceActivityQueue.Begin();
            }

            _pieces.Clear();
        }

        private class ActivityFinalize : Activity
        {
            private readonly Action _callback;

            public ActivityFinalize(Action callback)
            {
                _callback = callback;
            }

            public override void Begin()
            {
                base.Begin();
                _callback?.Invoke();
            }
        }

        public void ContinueTillEatOrStop(Action<Tile> done)
        {
            var t = _board.GetSuccessTile(CurrentTile, _forward);

            _boardTraveller.Reset();

            if (t.Pieces.Count > 0 && t is not MandarinTile)
            {
                Take(t.Pieces, t.Pieces.Count);
                SetMoveStartPoint(Array.IndexOf(_board.Tiles, t), _forward);

                foreach (var p in _pieces)
                {
                    PieceScheduler.CreateAnimActivity(p, () => LegHashes.stand_up, null);
                }

                DropAll(() => ContinueTillEatOrStop(done));
            }
            else
            {
                done?.Invoke(t);
            }
        }
    }
}