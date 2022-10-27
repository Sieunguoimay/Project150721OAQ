using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;

namespace Gameplay.Entities
{
    public sealed class Currency : Entity<ICurrencyData, ICurrencySavedData>, ICurrency
    {
        private double _amount;
        private IMessageService _messageService;

        public Currency(ICurrencyData data, ICurrencySavedData savedData) : base(data, savedData)
        {
            _amount = Data.InitialAmount;
        }

        public override void Inject(IResolver resolver)
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

        public bool CanRemove(double amount)
        {
            return amount <= _amount;
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

        private void DispatchMessage(double prevAmount)
        {
            _messageService.Dispatch<ICurrencyChangeMessage, ICurrency>(new CurrencyChangeMessage(this, prevAmount), this);
        }
    }

    public interface ICurrencyChangeMessage : IMessage<ICurrency>
    {
        double PrevAmount { get; }
    }

    public class CurrencyChangeMessage : AMessage<ICurrency>, ICurrencyChangeMessage
    {
        public CurrencyChangeMessage(ICurrency currency, double prevAmount) : base(currency)
        {
            PrevAmount = prevAmount;
        }

        public double PrevAmount { get; }
    }
}