using System;
using System.Linq;
using Common;
using Common.ResolveSystem;
using Gameplay.Board;
using SNM;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceManager : MonoBehaviour
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        private readonly Activity _waitForEnd = new();

        private Piece[] Pieces { get; set; }

        public void SpawnPieces(Board.Board board)
        {
            Pieces = new Piece[board.TileGroups.Sum(p => p.Tiles.Count * 5 + 1)];
            var count = 0;
            foreach (var tg in board.TileGroups)
            {
                Pieces[count] = Instantiate(mandarinPrefab, transform, true);
                Pieces[count].Setup();
                Pieces[count].transform.position = ((MandarinTile) tg.MandarinTile).GetPositionInFilledCircle(0);
                tg.MandarinTile.Grasp(Pieces[count]);

                count++;
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var p = Instantiate(citizenPrefab, transform, true);
                        p.Setup();
                        Pieces[count++] = p;
                    }
                }
            }
        }

        public void MoveToBoard(Board.Board board, bool triggerToGo)
        {
            var index = 0;
            foreach (var tg in board.TileGroups)
            {
                var delay = 0f;
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        if (Pieces[index] is Citizen p)
                        {
                            t.Grasp(p);
                            PieceScheduler.MovePieceToTheBoardOnGameStart(p, t as Tile, triggerToGo ? _waitForEnd : null, delay += 0.1f);
                        }
                        else
                        {
                            i--;
                        }

                        index++;
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace)
        {
            _waitForEnd.NotifyDone();
            this.WaitUntil(() => Pieces.All(p => p.PieceActivityQueue.Inactive), onAllInPlace);
        }

#if UNITY_EDITOR
        [ContextMenu("Move")]
        private void Move() => MoveToBoard(Resolver.Instance.Resolve<Board.Board>(), false);
#endif
    }
}