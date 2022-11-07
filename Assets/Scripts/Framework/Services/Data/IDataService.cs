using Framework.Entities;

namespace Framework.Services.Data
{
    public interface IDataService
    {
        TData Load<TData>(string id) where TData : IEntityData;
        object Load(string id);
    }
}