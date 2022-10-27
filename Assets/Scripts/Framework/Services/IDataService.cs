using Framework.Entities;

namespace Framework.Services
{
    public interface IDataService
    {
        TData Load<TData>(string id) where TData : IEntityData;
    }
}