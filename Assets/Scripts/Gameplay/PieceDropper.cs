using System;
using System.Collections.Generic;
using SNM;
using UnityEngine;
using Action = System.Action;

namespace Gameplay
{
    public class PieceDropper : PieceHolder
    {
        private BoardTraveller _boardTraveller;
        public bool IsTravelling => _boardTraveller?.IsTravelling ?? false;
        public event Action<IPieceHolder> OnEat = delegate { };
        public event Action<ActionID> OnDone = delegate { };

        private ActionID _actionID;
        private bool _forward;

        public void Setup(Board board)
        {
            if (_boardTraveller == null || _boardTraveller.Board != board)
            {
                _boardTraveller = new BoardTraveller(board, new BoardTraveller.Config {activeColor = Color.black});
            }
        }

        public void GetReady(Tile tile)
        {
            Grasp(tile);
            _boardTraveller.Start(tile, Pieces.Count);
            _actionID = ActionID.DroppingInTurn;
        }

        public void GetReadyForTakingBackCitizens(Board.TileGroup tileGroup, List<Piece> citizens)
        {
            var n = citizens.Count;
            for (var i = n - 1; i >= 0; i--)
            {
                if (n - i > tileGroup.Tiles.Count) break;

                var p = citizens[i];

                if (p is not Citizen) continue;
            
                Grasp(p);
                p.PieceActivityQueue.End();
                citizens.RemoveAt(i);
            }

            _boardTraveller.Start(tileGroup.MandarinTile, Pieces.Count);
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
                    var p = Pieces[i + j];

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new CommonActivities.Delay(delay));
                        delay += 0.2f;
                    }

                    var skipSlot = p is Citizen && _boardTraveller.CurrentTile is MandarinTile {HasMandarin: true};
                    var citizenPos = _boardTraveller.CurrentTile.GetPositionInFilledCircle(
                        _boardTraveller.CurrentTile.Pieces.Count + j + (skipSlot ? 5 : 0), false);
                    var flag = (i == n - 1) ? 2 : (j == 0 ? 1 : 0);

                    p.PieceScheduler.CreateNewJump(citizenPos, flag, OnJumpDone);
                }

                _boardTraveller.CurrentTile.Grasp(Pieces[i]);
            }

            foreach (var p in Pieces)
            {
                p.PieceScheduler.CreateNewLandAnim();
            }

            Pieces.Clear();
        }

        private void OnJumpDone(Piece last, int flag)
        {
            if (flag == 2)
            {
                OnDropAllDone();
            }
        }

        private void OnDropAllDone()
        {
            if (_actionID == ActionID.DroppingInTurn)
            {
                var t = _boardTraveller.CurrentTile.Success(_forward);
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
            }
            else if (_actionID == ActionID.TakingBack)
            {
                _boardTraveller.Reset();

                OnDone?.Invoke(_actionID);
            }
        }

        private void Eat(Tile tile, bool forward, Action done)
        {
            var success = tile.Success(forward);

            OnEat?.Invoke(success);

            if (CheckSuccessEatable(success.Success(forward), forward))
            {
                Main.Instance.Delay(0.2f, () => { Eat(success.Success(forward), forward, done); });
            }
            else
            {
                done?.Invoke();
            }
        }

        private static bool CheckSuccessEatable(Tile tile, bool forward)
        {
            var success = tile.Success(forward);

            return (tile.Pieces.Count == 0 && (!(tile is MandarinTile)) && (success.Pieces.Count > 0));
        }

        public enum ActionID
        {
            DroppingInTurn,
            TakingBack
        }
    }
}