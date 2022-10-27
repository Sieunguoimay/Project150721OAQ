using Framework.Resolver;

namespace Framework.Entities
{
    public interface IEntity<out TData, out TSavedData> : IInjectable where TData : IEntityData where TSavedData : IEntitySavedData
    {
        TData Data { get; }
        TSavedData SavedData { get; }

        void Initialize();
        void Terminate();
    }

    public interface IEntityData
    {
        string Id { get; }
        IEntity<IEntityData, IEntitySavedData> CreateEntity();
    }

    public interface IEntitySavedData
    {
        void Save();
    }

    public class EntityData : IEntityData
    {
        public EntityData(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new Entity<IEntityData, IEntitySavedData>(this, null);
        }
    }

    public class EntitySavedData : IEntitySavedData
    {
        public void Save()
        {
            
        }
    }

    public class Entity<TData, TSavedData> : IEntity<TData, TSavedData> where TData : IEntityData where TSavedData : IEntitySavedData
    {
        public Entity(TData data, TSavedData savedData)
        {
            Data = data;
            SavedData = savedData;
        }

        public virtual void Inject(IResolver resolver)
        {
        }

        public TData Data { get; }
        public TSavedData SavedData { get; }

        public virtual void Initialize()
        {
        }

        public virtual void Terminate()
        {
        }
    }
}