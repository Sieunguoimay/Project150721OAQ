using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using CommonActivities;
using Gameplay;
using SNM;
using UnityEngine;

namespace InGame
{
    public class PieceManager : MonoBehaviour
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        private readonly WaitForEnd _waitForEnd = new ();
        private Piece[] _pieces;

        public void SpawnPieces(Board board)
        {
            _pieces = new Piece[(board.Tiles.Length - board.TileGroups.Length) * 5 + board.TileGroups.Length];
            var count = 0;
            foreach (var tg in board.TileGroups)
            {
                _pieces[count] = Instantiate(mandarinPrefab, transform, true);
                _pieces[count].Setup();
                _pieces[count].transform.position = tg.MandarinTile.GetPositionInFilledCircle(0);
                tg.MandarinTile.Grasp(_pieces[count]);

                count++;
                var delay = 0f;
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var tf = tg.MandarinTile.transform;

                        var p = Instantiate(citizenPrefab, transform, true);
                        p.Setup();
                        p.transform.position = tf.position + tf.forward * 2f;
                        t.Grasp(p);
                        var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
                        p.PieceActivityQueue.Add(_waitForEnd);
                        p.PieceActivityQueue.Add(new Delay(delay += 0.1f));
                        p.PieceActivityQueue.Add(new Flocking(p.Config.flockingConfigData,
                            new Flocking.InputData {target = position, transform = p.transform}, null));

                        _pieces[count++] = p;
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace)
        {
            _waitForEnd.End();
            this.WaitUntil(() => _pieces.All(p => p.PieceActivityQueue.IsDone), onAllInPlace);
        }
    }
}