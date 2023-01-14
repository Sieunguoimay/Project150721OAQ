using System;
using Common.Activity;
using Framework.Resolver;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceManager : MonoControlUnitBase<PieceManager>
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        private Piece[] Pieces { get; set; }

        public void DeletePieces()
        {
            foreach (var p in Pieces)
            {
                Destroy(p.gameObject);
            }

            Pieces = null;
        }

        public void SpawnPieces(int groups, int tilesPerGroup, int numCitizen)
        {
            Pieces = new Piece[groups * tilesPerGroup * 5 + groups];
            var count = 0;
            for (var i = 0; i < groups; i++)
            {
                Pieces[count++] = Instantiate(mandarinPrefab, transform);
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < numCitizen; k++)
                    {
                        Pieces[count++] = Instantiate(citizenPrefab, transform);
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace, Board.Board board)
        {
            var index = 0;
            foreach (var tg in board.Sides)
            {
                foreach (var t in tg.CitizenTiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var p = Pieces[index];
                        if (p.Type == Piece.PieceType.Citizen)
                        {
                            t.PiecesContainer.Add(p);

                            var delay = i * 0.1f;
                            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.PiecesContainer.Count - 1));

                            p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                            p.ActivityQueue.Add(delay > 0 ? new ActivityDelay(delay) : null);
                            p.ActivityQueue.Add(new ActivityFlocking(p.FlockingConfigData, position, p.transform,
                                null));
                            p.ActivityQueue.Add(index == Pieces.Length - 1 ? new ActivityCallback(onAllInPlace) : null);
                            p.ActivityQueue.Begin();
                        }
                        else
                        {
                            p.transform.position = tg.MandarinTile.GetPositionInFilledCircle(0);
                            tg.MandarinTile.PiecesContainer.Add(Pieces[index]);
                            i--;
                        }

                        index++;
                    }
                }
            }
        }
    }
}