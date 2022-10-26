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
    public class OnScreenUI : MonoBehaviour, IInjectable
    {
        [SerializeField] private TextMeshProUGUI text;
        private GameFlowManager _flowManager;
        private IMessageHandler<ICurrencyChangeMessage, ICurrency> _currencyChangeHandler;

        public void Inject(IResolver resolver)
        {
            _flowManager = resolver.Resolve<GameFlowManager>();
            OnStateChanged();
            _currencyChangeHandler = new CurrencyChangeMessageHandler(this);
            var gameCurrency = resolver.Resolve<ICurrency>("game_currency");
            resolver.Resolve<IMessageService>().Register(_currencyChangeHandler, gameCurrency);
        }

        private class CurrencyChangeMessageHandler : InnerClass<OnScreenUI>,
            IMessageHandler<ICurrencyChangeMessage, ICurrency>
        {
            public CurrencyChangeMessageHandler(OnScreenUI context) : base(context)
            {
            }

            public void OnReceiveMessage(ICurrencyChangeMessage message)
            {
                Context.text.text = message.Sender.Get().ToString(CultureInfo.InvariantCulture);
            }
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