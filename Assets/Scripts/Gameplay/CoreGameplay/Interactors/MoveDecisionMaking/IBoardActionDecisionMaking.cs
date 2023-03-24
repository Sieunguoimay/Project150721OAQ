﻿using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public interface IBoardActionDecisionMaking
    {
        void MakeDecision(DecisionMakingData optionQueue, IBoardActionDecisionMakingResultHandler driver);

        void ForceEnd();

        public static BoardActionDecisionResultData CreateResultData(DecisionMakingData optionQueue)
        {
            var concurrentMoveSimulationInputData = CreateConcurrentSimulationInputData(optionQueue.OptionQueue);
            return new BoardActionDecisionResultData
            {
                SimulationInputData = concurrentMoveSimulationInputData,
                Success = true,
                ActionType = optionQueue.ActionType//GetActionType(optionQueue),
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

    public class DecisionMakingData
    {
        public OptionQueue OptionQueue;
        public BoardActionType ActionType;
    }
}