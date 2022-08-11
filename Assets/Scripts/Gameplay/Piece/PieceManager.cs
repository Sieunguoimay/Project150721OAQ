using System;
using System.Linq;
using Common;
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

        public void SpawnPieces(Player[] players)
        {
            Pieces = new Piece[players.Sum(p => p.TileGroup.Tiles.Count * 5 + 1)];
            var count = 0;
            foreach (var player in players)
            {
                var tg = player.TileGroup;
                Pieces[count] = Instantiate(mandarinPrefab, transform, true);
                Pieces[count].Setup();
                Pieces[count].transform.position = tg.MandarinTile.GetPositionInFilledCircle(0);
                tg.MandarinTile.Grasp(Pieces[count]);

                count++;
                var delay = 0f;
                foreach (var t in tg.Tiles)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var p = Instantiate(citizenPrefab, transform, true) as Citizen;
                        p.Setup();
                        t.Grasp(p);
                        
                        PieceScheduler.MovePieceToTheBoardOnGameStart(p, player.PieceBench.Config.PosAndRot.Position, t,
                            _waitForEnd,
                            delay += 0.1f);
                        Pieces[count++] = p;
                    }
                }
            }
        }

        public void ReleasePieces(Action onAllInPlace)
        {
            _waitForEnd.NotifyDone();
            this.WaitUntil(() => Pieces.All(p => p.PieceActivityQueue.Inactive), onAllInPlace);
        }
    }
}