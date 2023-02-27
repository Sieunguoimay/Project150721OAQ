using System;
using Gameplay.GameState;
using Gameplay.PlayTurn;

namespace Gameplay.DecisionMaking
{
    public interface IPlayerDecisionMakingController
    {
    }

    public class PlayerDecisionMakingController : BaseDependencyInversionScriptableObject,
        IPlayerDecisionMakingController
    {
        private IPlayTurnTeller _playTurnTeller;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _playTurnTeller = Resolver.Resolve<IPlayTurnTeller>();
            _playTurnTeller.TurnChangedEvent -= OnPlayTurnChanged;
            _playTurnTeller.TurnChangedEvent += OnPlayTurnChanged;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _playTurnTeller.TurnChangedEvent -= OnPlayTurnChanged;
        }

        private void OnPlayTurnChanged(IPlayTurnTeller playTurnTeller)
        {
            var decision = _playTurnTeller.CurrentTurn.DecisionMaking;
            decision.MakeDecision();
        }
    }
}