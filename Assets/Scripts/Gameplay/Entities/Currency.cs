using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;

namespace Gameplay.Entities
{
    public class Currency : ICurrency
    {
        private double _amount;
        private IMessageService _messageService;

        public void Inject(IResolver resolver)
        {
            _messageService = resolver.Resolve<IMessageService>();
        }

        public void Add(double amount)
        {
            Set(_amount + amount);
        }

        public void Remove(double amount)
        {
            Set(_amount - amount);
        }

        public void Set(double amount)
        {
            var prevAmount = _amount;
            _amount = amount;
            DispatchMessage(prevAmount);
        }

        public double Get()
        {
            return _amount;
        }

        protected virtual void DispatchMessage(double prevAmount)
        {
            _messageService.Dispatch<ICurrencyChangeMessage>(new CurrencyChangeMessage(this, prevAmount));
        }
    }

    public interface ICurrencyChangeMessage : IMessage
    {
        Currency Currency { get; }
        double PrevAmount { get; }
    }

    public class CurrencyChangeMessage : ICurrencyChangeMessage
    {
        public CurrencyChangeMessage(Currency currency, double prevAmount)
        {
            Currency = currency;
            PrevAmount = prevAmount;
        }

        public Currency Currency { get; }
        public double PrevAmount { get; }
    }
}