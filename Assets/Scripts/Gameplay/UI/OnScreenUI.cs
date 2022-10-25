using System;
using System.ResolveSystem;
using UnityEngine;

namespace Gameplay.UI
{
    public class OnScreenUI : MonoBehaviour, IInjectable
    {
        private GameFlowManager _flowManager;
        public void Inject(IResolver resolver)
        {
            _flowManager = resolver.Resolve<GameFlowManager>();
            OnStateChanged();
        }
     
        private void OnStateChanged()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                // transform.GetChild(i).gameObject.SetActive(_flowManager.CurrentState == GameFlowManager.GameState.DuringGameplay);
            }
        }

        public void OnFirstTap() => _flowManager.StartGame();
        public void OnReplayButtonClicked() => _flowManager.OnReplayMatch();
        public void OnHomeButtonClicked() => _flowManager.OnResetGame();

    }
}