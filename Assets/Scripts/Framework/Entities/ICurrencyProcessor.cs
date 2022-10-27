using Framework.Entities.Currency;
using Framework.Resolver;

namespace Framework.Entities
{
    public interface ICurrencyProcessor : IEntity<ICurrencyProcessorData, ICurrencyProcessorSavedData>
    {
        void Process();
    }

    public interface ICurrencyProcessorData : IEntityData
    {
        CurrencyAmount[] Inputs { get; }
        CurrencyAmount[] Outputs { get; }
    }

    public interface ICurrencyProcessorSavedData : IEntitySavedData
    {
    }
}