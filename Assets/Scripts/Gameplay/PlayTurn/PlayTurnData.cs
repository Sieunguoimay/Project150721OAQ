using System.Linq;
using Gameplay.Player;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.PlayTurn
{
    public class PlayTurnData
    {
        public int SideIndex { get; }

        public PlayTurnData(IPlayer player, BoardSideVisual boardSideVisual,
            PieceBench pieceBench, int sideIndex)
        {
            SideIndex = sideIndex;
            Player = player;
            BoardSideVisual = boardSideVisual;
            PieceBench = pieceBench;
        }

        private IPlayer Player { get; }
        private BoardSideVisual BoardSideVisual { get; }
        public PieceBench PieceBench { get; }

        public Transform[] GetCitizenTilesTransform()
        {
            return BoardSideVisual.CitizenTiles.Select(t => t.transform).ToArray();
        }
    }
}