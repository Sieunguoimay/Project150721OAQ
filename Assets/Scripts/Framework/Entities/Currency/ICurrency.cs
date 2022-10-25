namespace Framework.Entities.Currency
{
    public interface ICurrency
    {
        void Add(double amount);
        void Remove(double amount);
        void Set(double amount);
        double Get();
    }
}