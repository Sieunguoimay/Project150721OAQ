using System.Collections.Generic;
using System.Linq;
using Gameplay.DecisionMaking;
using Gameplay.Player;
using Gameplay.Visual;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.PlayTurn
{
    public class PlayTurnData
    {
        public int SideIndex { get; }

        public PlayTurnData(IPlayer player, BoardSideVisual boardSideVisual, IPlayerDecisionMaking decisionMaking,
            PieceBench pieceBench, int sideIndex)
        {
            SideIndex = sideIndex;
            Player = player;
            BoardSideVisual = boardSideVisual;
            DecisionMaking = decisionMaking;
            PieceBench = pieceBench;
        }

        private IPlayer Player { get; }
        private BoardSideVisual BoardSideVisual { get; }
        public IPlayerDecisionMaking DecisionMaking { get; }
        public PieceBench PieceBench { get; }

        public Transform[] GetCitizenTilesTransform()
        {
            return BoardSideVisual.CitizenTiles.Select(t => t.transform).ToArray();
        }
    }
}