using System;
using Common;
using Common.Activity;
using Common.UnityExtend;
using Gameplay.Piece.Activities;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.Piece
{
    public class PieceManager : MonoSelfBindingInjectable<PieceManager>
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        public Piece[] Pieces { get; private set; }
        private UnityObjectPooling<Piece> _mandarinPooling;
        private UnityObjectPooling<Piece> _citizenPooling;

        public void ResetAll()
        {
            foreach (var p in Pieces)
            {
                p.ActivityQueue.End();
                p.gameObject.SetActive(false);
            }
        }

        public void SpawnPieces(int groups, int tilesPerGroup)
        {
            _mandarinPooling ??= new UnityObjectPooling<Piece>(transform, mandarinPrefab, p => !p.gameObject.activeSelf);
            _citizenPooling ??= new UnityObjectPooling<Piece>(transform, citizenPrefab, p => !p.gameObject.activeSelf);

            if (Pieces == null)
            {
                Pieces = new Piece[groups * tilesPerGroup * 5 + groups];
            }
            else if(Pieces.Length!=groups * tilesPerGroup * 5 + groups)
            {
                var pieces = Pieces;
                Array.Resize(ref pieces,groups * tilesPerGroup * 5 + groups);
                Pieces = pieces;
            }

            var count = 0;
            for (var i = 0; i < groups; i++)
            {
                Pieces[count++] = _mandarinPooling.GetFromPool(); // Instantiate(mandarinPrefab, transform, true);
                Pieces[count-1].gameObject.SetActive(true);
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < 5; k++)
                    {
                        Pieces[count++] = _citizenPooling.GetFromPool(); //Instantiate(citizenPrefab, transform, true);
                        Pieces[count-1].gameObject.SetActive(true);
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