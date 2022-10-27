using System.Linq;
using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;

namespace Gameplay.Entities
{
    public class CurrencyProcessor : Entity<ICurrencyProcessorData, ICurrencyProcessorSavedData>, ICurrencyProcessor
    {
        private ICurrency[] _inputCurrencies;
        private ICurrency[] _outputCurrencies;
        private IMessageService _messageService;

        public CurrencyProcessor(ICurrencyProcessorData data, ICurrencyProcessorSavedData savedData) : base(data, savedData)
        {
        }

        public override void Inject(IResolver resolver)
        {
            _inputCurrencies = new ICurrency[Data.Inputs.Length];
            _outputCurrencies = new ICurrency[Data.Outputs.Length];

            for (var i = 0; i < _inputCurrencies.Length; i++)
            {
                _inputCurrencies[i] = resolver.Resolve<ICurrency>(Data.Inputs[i].CurrencyId);
            }

            for (var i = 0; i < _outputCurrencies.Length; i++)
            {
                _outputCurrencies[i] = resolver.Resolve<ICurrency>(Data.Outputs[i].CurrencyId);
            }

            _messageService = resolver.Resolve<IMessageService>();
        }

        public void Process()
        {
            var enoughInput = true;
            for (var i = 0; i < _inputCurrencies.Length; i++)
            {
                if (Data.Inputs[i].Amount > _inputCurrencies[i].Get())
                {
                    enoughInput = false;
                    break;
                }
            }

            if (enoughInput)
            {
                for (var i = 0; i < _inputCurrencies.Length; i++) _inputCurrencies[i].Remove(Data.Inputs[i].Amount);
                for (var i = 0; i < _outputCurrencies.Length; i++) _outputCurrencies[i].Add(Data.Outputs[i].Amount);
                _messageService.Dispatch<IMessage<ICurrencyProcessor>, ICurrencyProcessor>(new CurrencyProcessSuccess(this), this);
            }
        }

        private class CurrencyProcessSuccess : AMessage<ICurrencyProcessor>
        {
            public CurrencyProcessSuccess(ICurrencyProcessor sender) : base(sender)
            {
            }
        }
    }
}