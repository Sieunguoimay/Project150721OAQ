using Common;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.DecisionMaking
{
    public class DefaultDecisionMaking : IDecisionMaking
    {
        private IDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;

        public void MakeDecision(DecisionMakingData decisionMakingData, IDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _coroutine = PublicExecutor.Instance.Delay(1f, () =>
            {
                _driver.OnDecisionResult(this, CreateResultData(decisionMakingData));
            });
        }

        public void ForceEnd()
        {
            //Only for multi-frame decision making
            PublicExecutor.Instance.StopCoroutine(_coroutine);
        }

        private static DecisionResultData CreateResultData(DecisionMakingData decisionMakingData)
        {
            return new()
            {
                SimulationInputData = CreateRandomizedSimulationInputData(decisionMakingData),
                Success = true
            };
        }

        private static MoveSimulationInputData CreateRandomizedSimulationInputData(DecisionMakingData decisionMakingData)
        {
            return new()
            {
                Direction = true,
                SideIndex = decisionMakingData.TurnIndex,
                StartingTileIndex = decisionMakingData.Options[Random.Range(0, decisionMakingData.Options.Length)].TileIndex
            };
        }

    }
}