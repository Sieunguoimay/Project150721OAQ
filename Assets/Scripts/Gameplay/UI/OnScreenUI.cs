using System;
using Common.ResolveSystem;
using UnityEngine;

namespace Gameplay.UI
{
    public class OnScreenUI : MonoBehaviour, IInjectable
    {
        private GameFlowManager _flowManager;

        public void Bind(IResolver resolver)
        {

        }

        public void Setup(IResolver resolver)
        {
            _flowManager = resolver.Resolve<GameFlowManager>();
            OnStateChanged();
            _flowManager.StateChanged += OnStateChanged;
        }

        public void TearDown()
        {
            _flowManager.StateChanged -= OnStateChanged;
        }

        public void Unbind(IResolver resolver)
        {
        }

        private void OnStateChanged()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).
                    gameObject.SetActive(_flowManager.CurrentState == GameFlowManager.GameState.DuringGameplay);
            }
        }

        public void OnFirstTap() => _flowManager.StartGame();
        public void OnReplayButtonClicked() => _flowManager.OnReplayMatch();
        public void OnHomeButtonClicked() => _flowManager.OnResetGame();
    }
}