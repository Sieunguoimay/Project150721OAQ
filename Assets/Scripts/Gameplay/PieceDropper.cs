using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using CommonActivities;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;
using Action = System.Action;

namespace Gameplay
{
    public class PieceDropper : PieceHolder
    {
        private readonly BoardTraveller _boardTraveller = new();
        public event Action<IPieceHolder> OnEat = delegate { };
        public event Action<ActionID> OnDone = delegate { };

        private bool _forward;
        private ActionID _actionID;
        private Board.Board _board;
        private IPieceHolder CurrentTile => _board.Tiles[_boardTraveller.CurrentIndex];
        private Activity _lastPieceJumpActivity;
        private Coroutine _coroutine;

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

        public void Pickup(IPieceHolder tile)
        {
            Grasp(tile);
            _boardTraveller.Start(Array.IndexOf(_board.Tiles, tile), Pieces.Count, _board.Tiles.Length);
            _actionID = ActionID.DroppingInTurn;
        }

        public void GetReadyForTakingBackCitizens(Board.Board.TileGroup tileGroup, List<Piece.Piece> citizens)
        {
            var n = citizens.Count;
            for (var i = n - 1; i >= 0; i--)
            {
                if (n - i > tileGroup.Tiles.Length) break;

                var p = citizens[i];

                if (p is not Citizen) continue;

                Grasp(p);
                p.PieceActivityQueue.OnEnd();
                citizens.RemoveAt(i);
            }

            _boardTraveller.Start(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), Pieces.Count,
                _board.Tiles.Length);
            _actionID = ActionID.TakingBack;
        }

        public void DropAll(bool forward)
        {
            _forward = forward;
            var delay = 0f;
            var n = Pieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(forward);

                for (var j = 0; j < n - i; j++)
                {
                    if (Pieces[i + j] is not Citizen p) continue;

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new Delay(delay));
                        PieceScheduler.CreateAnimActivity(p,
                            () => p.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == LegHashes.idle
                                ? LegHashes.jump_interval
                                : LegHashes.stand_up, null);
                        delay += 0.2f;
                    }

                    var skipSlot = CurrentTile is MandarinTile {HasMandarin: true};
                    var citizenPos =
                        ((Tile) CurrentTile).GetPositionInFilledCircle(
                            CurrentTile.Pieces.Count + j + (skipSlot ? 5 : 0), false);

                    var jumpForward = new JumpForward(p.transform, citizenPos, .4f, new LinearEasing(), 1f,
                        BezierEasing.CreateBezierEasing(0.35f, 0.75f));

                    p.PieceActivityQueue.Add(jumpForward);

                    if (j > 0)
                    {
                        PieceScheduler.CreateAnimActivity(p, LegHashes.jump_interval, null);
                    }

                    if (i == n - 1)
                    {
                        _lastPieceJumpActivity = jumpForward;
                        _lastPieceJumpActivity.Done += OnDropAllDone;
                    }
                }

                CurrentTile.Grasp(Pieces[i]);
            }

            foreach (var p in Pieces)
            {
                PieceScheduler.CreateAnimActivity(p, LegHashes.land, () => p.Animator.Play(LegHashes.idle));
                p.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(p.transform));
                PieceScheduler.CreateAnimActivity(p, LegHashes.sit_down, null);
                p.PieceActivityQueue.Begin();
            }

            Pieces.Clear();
        }

        private void OnDropAllDone()
        {
            _lastPieceJumpActivity = null;
            switch (_actionID)
            {
                case ActionID.DroppingInTurn:
                {
                    var t = _board.GetSuccessTile(CurrentTile, _forward);

                    _boardTraveller.Reset();

                    if (t.Pieces.Count > 0 && t is not MandarinTile)
                    {
                        Pickup(t);
                        _coroutine = PublicExecutor.Instance.Delay(.3f, () =>
                        {
                            _coroutine = null;
                            DropAll(_forward);
                        });
                    }
                    else
                    {
                        if (CheckSuccessEatable(t, _forward))
                        {
                            Eat(t, _forward, () => { OnDone?.Invoke(_actionID); });
                        }
                        else
                        {
                            OnDone?.Invoke(_actionID);
                        }
                    }

                    break;
                }
                case ActionID.TakingBack:
                    _boardTraveller.Reset();

                    OnDone?.Invoke(_actionID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Eat(IPieceHolder tile, bool forward, Action done)
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

        private bool CheckSuccessEatable(IPieceHolder tile, bool forward)
        {
            return tile.Pieces.Count == 0 && tile is not MandarinTile &&
                   _board.GetSuccessTile(tile, forward).Pieces.Count > 0;
        }

        public enum ActionID
        {
            DroppingInTurn,
            TakingBack
        }
    }
}