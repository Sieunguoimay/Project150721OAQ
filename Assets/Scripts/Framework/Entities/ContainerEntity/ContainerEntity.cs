using Framework.Entities;

namespace Framework.Entities.ContainerEntity
{
    public interface IContainerEntity<out TData, out TSavedData> : IEntity<TData, TSavedData>
        where TData : IContainerEntityData where TSavedData : IContainerEntitySavedData
    {
        IEntity<IEntityData, IEntitySavedData>[] Components { get; }
    }

    public class ContainerEntity<TData, TSavedData> : BaseEntity<TData, TSavedData>, IContainerEntity<TData, TSavedData>
        where TData : IContainerEntityData where TSavedData : IContainerEntitySavedData
    {
        public ContainerEntity(TData data, TSavedData savedData,
            IEntity<IEntityData, IEntitySavedData>[] components) : base(data, savedData)
        {
            Components = components;
        }

        public IEntity<IEntityData, IEntitySavedData>[] Components { get; }
    }
}