﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly BoardTraveller _boardTraveller = new BoardTraveller();
        public event Action<IPieceHolder> OnEat = delegate { };
        public event Action<ActionID> OnDone = delegate { };

        private ActionID _actionID;
        private bool _forward;
        private Board.Board _board;
        private Tile CurrentTile => _board.Tiles[_boardTraveller.CurrentIndex];

        public void Setup(Board.Board board)
        {
            _board = board;
        }

        public void GetReady(Tile tile)
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
                if (n - i > tileGroup.Tiles.Count) break;

                var p = citizens[i];

                if (p is not Citizen) continue;

                Grasp(p);
                p.PieceActivityQueue.OnEnd();
                citizens.RemoveAt(i);
            }

            _boardTraveller.Start(Array.IndexOf(_board.Tiles, tileGroup.MandarinTile), Pieces.Count, _board.Tiles.Length);
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
                    if (!(Pieces[i + j] is Citizen p)) continue;

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new Delay(delay)); // + 0.1f));
                        PieceScheduler.CreateAAnimActivity(p, () =>
                        {
                            var info = p.Animator.GetCurrentAnimatorStateInfo(0);
                            return info.shortNameHash == LegHashes.idle
                                ? LegHashes.jump_interval
                                : LegHashes.stand_up;
                        }, null);
                        delay += 0.2f;
                    }

                    var skipSlot = CurrentTile is MandarinTile {HasMandarin: true};
                    var citizenPos = CurrentTile.GetPositionInFilledCircle(
                        CurrentTile.Pieces.Count + j + (skipSlot ? 5 : 0), false);
                    var flag = i == n - 1 ? 2 : j == 0 ? 1 : 0;

                    var jumpForward = new JumpForward(p.transform, citizenPos, .4f,
                        new LinearEasing(), 1f, BezierEasing.CreateBezierEasing(0.35f, 0.75f));

                    p.PieceActivityQueue.Add(jumpForward);

                    if (j > 0)
                    {
                        PieceScheduler.CreateAAnimActivity(p, LegHashes.jump_interval, null);
                    }

                    jumpForward.Done += () => OnJumpDone(p, flag);
                }

                CurrentTile.Grasp(Pieces[i]);
            }

            foreach (var p in Pieces.Cast<Citizen>())
            {
                PieceScheduler.CreateAAnimActivity(p, LegHashes.land, () => p.Animator.Play(LegHashes.idle));
                p.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(p.transform));
                PieceScheduler.CreateAAnimActivity(p, LegHashes.sit_down, null);
                p.PieceActivityQueue.Begin();
            }

            Pieces.Clear();
        }

        private void OnJumpDone(Piece.Piece last, int flag)
        {
            if (flag == 2)
            {
                OnDropAllDone();
            }
        }

        private void OnDropAllDone()
        {
            switch (_actionID)
            {
                case ActionID.DroppingInTurn:
                {
                    var t = _board.Success(CurrentTile, _forward);
                    _boardTraveller.Reset();

                    if (t.Pieces.Count > 0 && !(t is MandarinTile))
                    {
                        GetReady(t);
                        Main.Instance.Delay(.3f, () => { DropAll(_forward); });
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

        private void Eat(Tile tile, bool forward, Action done)
        {
            var success = _board.Success(tile, forward);

            OnEat?.Invoke(success);

            if (CheckSuccessEatable(_board.Success(success, forward), forward))
            {
                Main.Instance.Delay(0.2f, () => { Eat(_board.Success(success, forward), forward, done); });
            }
            else
            {
                done?.Invoke();
            }
        }

        private bool CheckSuccessEatable(Tile tile, bool forward)
        {
            var success = _board.Success(tile, forward);

            return (tile.Pieces.Count == 0 && (!(tile is MandarinTile)) && (success.Pieces.Count > 0));
        }

        public enum ActionID
        {
            DroppingInTurn,
            TakingBack
        }
    }
}