using System;
using System.Globalization;
using Framework;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;
using Gameplay.Entities;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    [Obsolete]
    public class OnScreenUI : MonoBehaviour, IInjectable
    {
        [SerializeField] private TextMeshProUGUI text;
        private IResolver _resolver;

        public void Inject(IResolver resolver)
        {
            _resolver = resolver;
            OnStateChanged();
            var gameCurrency = resolver.Resolve<ICurrency>("game_currency");
            resolver.Resolve<IMessageService>().Register<ICurrencyChangeMessage, ICurrency>(OnReceiveMessage, gameCurrency);
        }

        public void OnReceiveMessage(ICurrencyChangeMessage message)
        {
            text.text = message.Sender.Get().ToString(CultureInfo.InvariantCulture);
        }


        private void OnStateChanged()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                // transform.GetChild(i).gameObject.SetActive(_flowManager.CurrentState == GameFlowManager.GameState.DuringGameplay);
            }
        }

        public void OnReplayButtonClicked()
        {
        }

        public void OnHomeButtonClicked()
        {
        }
#if UNITY_EDITOR
        [ContextMenu("Test")]
        void Test()
        {
            var gameCurrency = _resolver.Resolve<ICurrency>("game_currency");
            gameCurrency.Add(1);
        }
#endif
    }
}