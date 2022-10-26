using Framework.Resolver;

namespace Framework.Entities.Currency
{
    public interface ICurrency : IInjectable
    {
        void Add(double amount);
        void Remove(double amount);
        void Set(double amount);
        double Get();
    }
}