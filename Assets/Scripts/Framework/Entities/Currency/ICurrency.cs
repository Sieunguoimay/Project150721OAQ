using Framework.Resolver;

namespace Framework.Entities.Currency
{
    public interface ICurrency : IEntity<ICurrencyData, ICurrencySavedData>
    {
        void Add(double amount);
        void Remove(double amount);
        bool CanRemove(double amount);
        void Set(double amount);
        double Get();
    }

    public interface ICurrencyData : IEntityData
    {
    }

    public interface ICurrencySavedData : IEntitySavedData
    {
        double Get();
        void Set(double amount);
    }
}