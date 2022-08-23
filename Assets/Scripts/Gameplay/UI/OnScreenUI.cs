using System;
using Common.ResolveSystem;
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

        private void Awake()
        {
            _flowManager.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            _flowManager.StateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if (_flowManager.CurrentState == GameFlowManager.GameState.BeforeGameplay)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnFirstTap();
                }
            }
        }

        private void OnStateChanged()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).
                    gameObject.SetActive(_flowManager.CurrentState == GameFlowManager.GameState.DuringGameplay);
            }
        }

        public void OnFirstTap() => _flowManager.OnStartGame();
        public void OnReplayButtonClicked() => _flowManager.OnReplayMatch();
        public void OnHomeButtonClicked() => _flowManager.OnResetGame();
    }
}