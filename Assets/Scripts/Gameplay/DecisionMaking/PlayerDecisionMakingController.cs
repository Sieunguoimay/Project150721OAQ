using System;
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

        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
        }

        public void OnGameStart()
        {
            _playTurnTeller = Resolver.Resolve<IGameplayContainer>().PlayTurnTeller;
            _playTurnTeller.TurnChangedEvent -= OnPlayTurnChanged;
            _playTurnTeller.TurnChangedEvent += OnPlayTurnChanged;
        }

        public void OnGameEnd()
        {
            _playTurnTeller.TurnChangedEvent -= OnPlayTurnChanged;
        }

        private void OnPlayTurnChanged(IPlayTurnTeller playTurnTeller)
        {
            var decision = _playTurnTeller.CurrentTurn.DecisionMaking;
            decision.MakeDecision();
        }
    }
}