using System.Linq;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class PlayerMoveDecisionMaking : IMoveDecisionMaking
    {
        private readonly InteractSystem _interactSystem;
        private IMoveDecisionMakingResultHandler _driver;

        public PlayerMoveDecisionMaking(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public void MakeDecision(MoveDecisionMakingData moveDecisionMakingData, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            var options = moveDecisionMakingData.Options.Select(o => o as SimpleMoveOption).ToArray();
            _interactSystem.DisplayOptions(options, OnInteractResult);
        }

        private void OnInteractResult(SimpleMoveOption obj)
        {
            _driver.OnDecisionResult(this, new MoveDecisionResultData()
            {
                SimulationInputData = new MoveSimulationInputData(),
                Success = true
            });
        }

        public void ForceEnd()
        {
            _interactSystem.Dismiss();
        }
    }
}