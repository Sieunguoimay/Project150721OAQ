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
            _messageService.Dispatch<ICurrencyChangeMessage,ICurrency>(new CurrencyChangeMessage(this, prevAmount));
        }
    }

    public interface ICurrencyChangeMessage : IMessage<ICurrency>
    {
        double PrevAmount { get; }
    }

    public class CurrencyChangeMessage : ICurrencyChangeMessage
    {
        public CurrencyChangeMessage(ICurrency currency, double prevAmount)
        {
            Sender = currency;
            PrevAmount = prevAmount;
        }
        public double PrevAmount { get; }
        public ICurrency Sender { get; }
    }
}