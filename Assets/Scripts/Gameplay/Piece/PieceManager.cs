using System;
using System.Linq;
using Common;
using Common.ResolveSystem;
using Gameplay.Board;
using SNM;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceManager : InjectableBehaviour<PieceManager>
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        public Piece[] Pieces { get; set; }

        public void ResetAll()
        {
            foreach (var p in Pieces)
            {
                p.PieceActivityQueue.NotifyDone();
                p.PieceActivityQueue.Activities.Clear();
            }
        }
        
        public void SpawnPieces(int groups, int tilesPerGroup)
        {
            Pieces = new Piece[groups * tilesPerGroup * 5 + groups];
            var count = 0;
            for (var i = 0; i < groups; i++)
            {
                Pieces[count] = Instantiate(mandarinPrefab, transform, true);
                Pieces[count].Setup();

                count++;
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < 5; k++)
                    {
                        var p = Instantiate(citizenPrefab, transform, true);
                        p.Setup();
                        Pieces[count++] = p;
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace, Board.Board board)
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
                            PieceScheduler.MovePieceToTheBoardOnGameStart(p, t as Tile, null, delay += 0.1f);
                        }
                        else
                        {
                            Pieces[index].transform.position =
                                ((MandarinTile) tg.MandarinTile).GetPositionInFilledCircle(0);
                            tg.MandarinTile.Grasp(Pieces[index]);
                            i--;
                        }

                        index++;
                    }
                }
            }

            this.WaitUntil(() => Pieces.All(p => p.PieceActivityQueue.Inactive), onAllInPlace);
        }

    }
}