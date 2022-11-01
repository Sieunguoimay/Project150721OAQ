using System;
using Common;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceManager : MonoBindingInjectable<PieceManager>
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        public Piece[] Pieces { get; private set; }

        public void ResetAll()
        {
            foreach (var p in Pieces)
            {
                p.ActivityQueue.MarkAsDone();
                p.ActivityQueue.End();
            }
        }

        public void SpawnPieces(int groups, int tilesPerGroup)
        {
            Pieces = new Piece[groups * tilesPerGroup * 5 + groups];

            var count = 0;
            for (var i = 0; i < groups; i++)
            {
                Pieces[count++] = Instantiate(mandarinPrefab, transform, true);

                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < 5; k++)
                    {
                        Pieces[count++] = Instantiate(citizenPrefab, transform, true);
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace, Board.Board board)
        {
            var index = 0;
            foreach (var tg in board.TileGroups)
            {
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        if (Pieces[index] is Citizen p)
                        {
                            t.Pieces.Add(p);

                            var delay = i * 0.1f;
                            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));

                            p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                            p.ActivityQueue.Add(delay > 0 ? new ActivityDelay(delay) : null);
                            p.ActivityQueue.Add(new ActivityFlocking(p.FlockingConfigData, position, p.transform,
                                null));
                            p.ActivityQueue.Add(index == Pieces.Length - 1
                                ? new ActivityCallback(onAllInPlace)
                                : null);
                            p.ActivityQueue.Begin();
                        }
                        else
                        {
                            Pieces[index].transform.position = tg.MandarinTile.GetPositionInFilledCircle(0);
                            tg.MandarinTile.Pieces.Add(Pieces[index]);
                            i--;
                        }

                        index++;
                    }
                }
            }
        }

    }
}