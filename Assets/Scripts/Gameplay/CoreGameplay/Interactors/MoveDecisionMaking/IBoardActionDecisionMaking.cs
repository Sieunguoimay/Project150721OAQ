using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.OptionSystem;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public interface IBoardActionDecisionMaking
    {
        void MakeDecision(OptionQueue optionQueue, IBoardActionDecisionMakingResultHandler driver);

        void ForceEnd();

        public static BoardActionDecisionResultData CreateResultData(OptionQueue optionQueue)
        {
            var concurrentMoveSimulationInputData = CreateConcurrentSimulationInputData(optionQueue);
            return new BoardActionDecisionResultData
            {
                SimulationInputData = concurrentMoveSimulationInputData,
                Success = true,
                ActionType = GetActionType(optionQueue),
            };
        }

        private static BoardActionType GetActionType(OptionQueue optionQueue)
        {
            if (optionQueue.Options.Count(o => o is TileOptionItem) > 1)
            {
                return BoardActionType.Concurrent;
            }

            return BoardActionType.GoneWithTheWind;
        }

        public static MoveSimulationInputData CreateConcurrentSimulationInputData(
            OptionQueue optionQueue)
        {
            var direction = false;
            var tileIndices = new List<int>();
            foreach (var moveOptionItem in optionQueue.Options)
            {
                switch (moveOptionItem)
                {
                    case DirectionOptionItem directionOptionItem:
                        direction = directionOptionItem.SelectedDirection;
                        break;
                    case TileOptionItem tileOptionItem:
                        tileIndices.Add(tileOptionItem.SelectedTileIndex);
                        break;
                }
            }

            if (tileIndices.Count == 0)
            {
                Debug.LogError("Must provide a tileOption");
            }

            return new MoveSimulationInputData
            {
                Direction = direction,
                SideIndex = optionQueue.TurnIndex,
                StartingTileIndices = tileIndices.ToArray(),
                StartingTileIndex = tileIndices[0],
            };
        }
    }
}