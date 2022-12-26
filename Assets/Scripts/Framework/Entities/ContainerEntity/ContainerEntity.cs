using Framework.Entities;

namespace Framework.Entities.ContainerEntity
{
    public interface IContainerEntity : IEntity<IContainerEntityData, IContainerEntitySavedData>
    {
        IEntity<IEntityData,IEntitySavedData>[] Components { get; }
    }

    public class ContainerEntity : BaseEntity<IContainerEntityData, IContainerEntitySavedData>, IContainerEntity
    {
        public ContainerEntity(IContainerEntityData data, IContainerEntitySavedData savedData, IEntity<IEntityData, IEntitySavedData>[] components) : base(data, savedData)
        {
            Components = components;
        }

        public IEntity<IEntityData, IEntitySavedData>[] Components { get; }
    }
}