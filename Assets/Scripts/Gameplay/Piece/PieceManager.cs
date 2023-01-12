using System;
using Common.Activity;
using Gameplay.Piece.Activities;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.Piece
{
    public class PieceManager : MonoSelfBindingInjectable<PieceManager>
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        private Piece[] Pieces { get; set; }
        // private ObjectPool<Piece> _mandarinPool;
        // private ObjectPool<Piece> _citizenPool;

        protected override void SetupInternal()
        {
            base.SetupInternal();
            //
            // _mandarinPool = new ObjectPool<Piece>(() => Instantiate(mandarinPrefab, transform), ActionOnGet,
            //     ActionOnRelease);
            //
            // _citizenPool = new ObjectPool<Piece>(() => Instantiate(citizenPrefab, transform), ActionOnGet,
            //     ActionOnRelease);
            //
            // static void ActionOnGet(Component p)
            // {
            //     p.gameObject.SetActive(true);
            // }
            //
            // static void ActionOnRelease(Component p)
            // {
            //     p.gameObject.SetActive(false);
            // }
        }

        public void ResetAll()
        {
            foreach (var p in Pieces)
            {
                // p.ActivityQueue.End();
                // switch (p.Type)
                // {
                //     case Piece.PieceType.Citizen:
                //         _citizenPool.Release(p);
                //         break;
                //     case Piece.PieceType.Mandarin:
                //         _mandarinPool.Release(p);
                //         break;
                //     default:
                //         throw new ArgumentOutOfRangeException();
                // }
                Destroy(p.gameObject);
            }

            Pieces = null;
        }

        public void SpawnPieces(int groups, int tilesPerGroup)
        {
            Pieces = new Piece[groups * tilesPerGroup * 5 + groups];
            var count = 0;
            for (var i = 0; i < groups; i++)
            {
                Pieces[count++] = Instantiate(mandarinPrefab,transform);//_mandarinPool.Get();
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < 5; k++)
                    {
                        Pieces[count++] = Instantiate(citizenPrefab,transform);//_citizenPool.Get();
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
                            p.ActivityQueue.Add(index == Pieces.Length - 1 ? new ActivityCallback(onAllInPlace) : null);
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