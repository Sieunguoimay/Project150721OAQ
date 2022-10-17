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
        public List<Piece.Piece> Pieces { get; } = new();

        private readonly BoardTraveller _boardTraveller = new();
        public event Action<Tile> OnEat = delegate { };

        private bool _forward;
        private Board.Board _board;
        private Tile CurrentTile => _board.Tiles[_boardTraveller.CurrentIndex];
        private Activity _lastPieceJumpActivity;
        private Coroutine _coroutine;
        private Action _done;

        public void Setup(Board.Board board)
        {
            _board = board;
        }

        public void Reset()
        {
            if (_lastPieceJumpActivity != null)
            {
                _lastPieceJumpActivity.Done -= OnDropAllDone;
                _lastPieceJumpActivity = null;
            }

            if (_coroutine != null)
            {
                PublicExecutor.Instance.StopCoroutine(_coroutine);
            }

            Pieces.Clear();
        }

        // public void PickupTile(Tile tile)
        // {
        //     ForwardLastItems(tile.Pieces, Pieces, tile.Pieces.Count);
        //
        //     // Pieces.AddRange(tile.Pieces);
        //     // tile.Pieces.Clear();
        //
        //     _boardTraveller.Start(Array.IndexOf(_board.Tiles, tile), Pieces.Count, _board.Tiles.Length);
        //
        //     _actionID = ActionID.DroppingInTurn;
        // }
        //
        // public void GetReadyForTakingBackCitizens(Board.Board.TileGroup tileGroup, List<Piece.Piece> citizens)
        // {
        //     ForwardLastItems(citizens, Pieces, tileGroup.Tiles.Length);
        //
        //     // var n = citizens.Count;
        //     // for (var i = 0; i < tileGroup.Tiles.Length; i++)
        //     // {
        //     //     var p = citizens[n - i - 1];
        //     //
        //     //     if (p is not Citizen) continue;
        //     //
        //     //     Pieces.Add(p);
        //     //
        //     //     p.PieceActivityQueue.End();
        //     //     citizens.Remove(p);
        //     // }
        //
        //     _boardTraveller.Start(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), Pieces.Count,
        //         _board.Tiles.Length);
        //
        //     _actionID = ActionID.TakingBack;
        // }

        public void Take(List<Piece.Piece> pieces, int num)
        {
            ForwardLastItems(pieces, Pieces, num);
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, Pieces.Count, _board.Tiles.Length);
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
            _done = done;
            var delay = 0f;
            var n = Pieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);

                for (var j = 0; j < n - i; j++)
                {
                    if (Pieces[i + j] is not Citizen p) continue;

                    var skipSlot = CurrentTile is MandarinTile;
                    var citizenPos =
                        CurrentTile.GetPositionInFilledCircle(CurrentTile.Pieces.Count + j + (skipSlot ? 5 : 0));

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new ActivityRotatePiece(citizenPos, p, delay));

                        delay += 0.2f;
                    }

                    var jumpForward = PieceScheduler.CreateJumpTimelineActivity(p, citizenPos);

                    if (i == n - 1)
                    {
                        _lastPieceJumpActivity = jumpForward;
                        _lastPieceJumpActivity.Done += OnDropAllDone;
                    }
                }

                CurrentTile.Pieces.Add(Pieces[i]);
            }

            foreach (var p in Pieces)
            {
                p.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(p.transform));
                PieceScheduler.CreateAnimActivity(p, LegHashes.sit_down, null);
                p.PieceActivityQueue.Begin();
            }

            Pieces.Clear();
        }

        private void OnDropAllDone()
        {
            _done?.Invoke();
            // if (_lastPieceJumpActivity != null)
            // {
            //     _lastPieceJumpActivity.Done -= OnDropAllDone;
            _lastPieceJumpActivity = null;
            // }
            // return;
            // switch (_actionID)
            // {
            //     case ActionID.DroppingInTurn:
            //     {
            //         Continue();
            //         break;
            //     }
            //     case ActionID.TakingBack:
            //         _boardTraveller.Reset();
            //
            //         OnDone?.Invoke(_actionID);
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
        }

        public void Continue(Action done)
        {
            var t = _board.GetSuccessTile(CurrentTile, _forward);

            _boardTraveller.Reset();

            if (t.Pieces.Count > 0 && t is not MandarinTile)
            {
                Take(t.Pieces, t.Pieces.Count);
                SetMoveStartPoint(Array.IndexOf(_board.Tiles, t), _forward);

                foreach (var p in Pieces)
                {
                    PieceScheduler.CreateAnimActivity(p, () => LegHashes.stand_up, null);
                }

                DropAll(() => Continue(done));
            }
            else
            {
                if (CheckSuccessEatable(t, _forward))
                {
                    Eat(t, _forward, () => { done?.Invoke(); });
                }
                else
                {
                    done?.Invoke();
                }
            }
        }

        private void Eat(Tile tile, bool forward, Action done)
        {
            var success = _board.GetSuccessTile(tile, forward);

            OnEat?.Invoke(success);

            if (CheckSuccessEatable(_board.GetSuccessTile(success, forward), forward))
            {
                _coroutine = PublicExecutor.Instance.Delay(0.2f,
                    () =>
                    {
                        _coroutine = null;
                        Eat(_board.GetSuccessTile(success, forward), forward, done);
                    });
            }
            else
            {
                done?.Invoke();
            }
        }

        private bool CheckSuccessEatable(Tile tile, bool forward)
        {
            return tile.Pieces.Count == 0 && tile is not MandarinTile &&
                   _board.GetSuccessTile(tile, forward).Pieces.Count > 0;
        }
    }
}