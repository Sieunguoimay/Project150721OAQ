using System;
using System.Collections.Generic;
using Common;
using CommonActivities;
using Gameplay;
using UnityEngine;

namespace InGame
{
    public class PieceManager : MonoBehaviour
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;

        private readonly WaitForEnd _waitForEnd = new();

        public void SpawnPieces(Board board)
        {
            foreach (var tg in board.TileGroups)
            {
                Spawn(mandarinPrefab, tg.MandarinTile);
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var p = Spawn(citizenPrefab, t);
                        var tf = tg.MandarinTile.transform;
                        p.transform.position = tf.position + tf.right * 2f;
                    }
                }
            }
        }

        private Piece Spawn(Piece prefab, PieceContainer t)
        {
            var p = Instantiate(prefab, transform, true);
            p.Setup();

            t.Grasp(p);
            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
            p.PieceActivityQueue.Add(_waitForEnd);
            p.PieceActivityQueue.Add(new Flocking(p.Config.flockingConfigData,
                new Flocking.InputData {target = position, transform = p.transform}, null));
            return p;
        }

        [ContextMenu("ReleasePieces")]
        public void ReleasePieces()
        {
            _waitForEnd.End();
        }
    }
}