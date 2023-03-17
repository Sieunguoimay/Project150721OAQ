using System;
using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;

namespace Gameplay.Entities
{
    public sealed class Currency : BaseEntity<ICurrencyData, ICurrencySavedData>, ICurrency
    {
        private IMessageService _messageService;

        public Currency(ICurrencyData data, ICurrencySavedData savedData) : base(data, savedData)
        {
        }

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _messageService = resolver.Resolve<IMessageService>();
        }

        public void Add(double amount)
        {
            Set(SavedData.Get() + amount);
        }

        public void Remove(double amount)
        {
            Set(SavedData.Get() - amount);
        }

        public bool CanRemove(double amount)
        {
            return amount <= SavedData.Get();
        }

        public void Set(double amount)
        {
            var prevAmount = SavedData.Get();
            SavedData.Set(amount);
            _messageService.Dispatch("CurrencyChangeMessage", this, new CurrencyChangeMessageArgs(prevAmount));
        }

        public double Get() => SavedData.Get();
    }

    public interface ICurrencyChangeMessageArgs
    {
        double PrevAmount { get; }
    }

    public class CurrencyChangeMessageArgs : EventArgs, ICurrencyChangeMessageArgs
    {
        public CurrencyChangeMessageArgs(double prevAmount)
        {
            PrevAmount = prevAmount;
        }

        public double PrevAmount { get; }
    }
}