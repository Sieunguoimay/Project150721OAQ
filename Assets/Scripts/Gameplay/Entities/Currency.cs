using Framework.Entities.Currency;

namespace Gameplay.Entities
{
    public abstract class Currency : ICurrency
    {
        private double _amount;

        public void Add(double amount)
        {
            _amount += amount;
        }

        public void Remove(double amount)
        {
            _amount -= amount;
        }

        public void Set(double amount)
        {
            _amount = amount;
        }

        public double Get()
        {
            return _amount;
        }
    }
}