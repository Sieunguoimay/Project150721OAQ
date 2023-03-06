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
        private readonly BoardStateView _boardStateView;
        public int SideIndex { get; }

        public PlayTurnData(IPlayer player, BoardSideVisual boardSideVisual, IPlayerDecisionMaking decisionMaking,
            PieceBench pieceBench, BoardStateView boardStateView, int sideIndex)
        {
            _boardStateView = boardStateView;
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

        public bool AnyCitizenTileHasPieces()
        {
            return _boardStateView.CheckAnyCitizenTileOnSideHasPieces(SideIndex);
            // return BoardSide.CitizenTiles.Any(t => t.HeldPieces.Count > 0);
        }

        public bool AnyPieceOnBench()
        {
            return _boardStateView.CheckBenchOnSideHasPieces(SideIndex);
            // return PieceBench.HeldPieces.Count > 0;
        }

        public Transform[] GetCitizenTilesTransform()
        {
            return BoardSideVisual.CitizenTiles.Select(t => t.transform).ToArray();
        }
    }
}