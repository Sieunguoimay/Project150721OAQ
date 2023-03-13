using Common;
using Gameplay.CoreGameplay.Interactors.Simulation;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DefaultMoveDecisionMaking : IMoveDecisionMaking
    {
        private IMoveDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;

        public void MakeDecision(MoveDecisionMakingData moveDecisionMakingData, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _coroutine = PublicExecutor.Instance.Delay(1f,
                () => { _driver.OnDecisionResult(this, CreateResultData(moveDecisionMakingData)); });
        }

        public void ForceEnd()
        {
            //Only for multi-frame decision making
            PublicExecutor.Instance.StopCoroutine(_coroutine);
        }

        private static MoveDecisionResultData CreateResultData(MoveDecisionMakingData moveDecisionMakingData)
        {
            return new()
            {
                SimulationInputData = CreateRandomizedSimulationInputData(moveDecisionMakingData),
                Success = true
            };
        }

        private static MoveSimulationInputData CreateRandomizedSimulationInputData(
            MoveDecisionMakingData moveDecisionMakingData)
        {
            var option = moveDecisionMakingData.Options[Random.Range(0, moveDecisionMakingData.Options.Length)];
            
            if (option is SimpleMoveOption so)
            {
                return new()
                {
                    Direction = so.Direction,
                    SideIndex = moveDecisionMakingData.TurnIndex,
                    StartingTileIndex = so.TileIndex
                };
            }

            return null;
        }
    }
}