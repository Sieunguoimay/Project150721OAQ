using Framework.Resolver;
using Framework.Services;

namespace Framework.Entities
{
    public interface IEntityLoader : IInjectable
    {
        IEntity<IEntityData, IEntitySavedData> CreateEntity<TEntity, TEntityData>(string entityDataId) where TEntityData : IEntityData where TEntity : IEntity<IEntityData, IEntitySavedData>;
        void DestroyEntity<TEntity>(string entityDataId) where TEntity : IEntity<IEntityData, IEntitySavedData>;
    }

    //Only for entity
    public class EntityLoader : IEntityLoader
    {
        private IDataService _dataService;
        private IResolver _resolver;
        private IBinder _binder;

        public void Inject(IResolver resolver)
        {
            _resolver = resolver;
            _dataService = resolver.Resolve<IDataService>();
            _binder = resolver.Resolve<IBinder>();
        }

        public IEntity<IEntityData, IEntitySavedData> CreateEntity<TEntity, TEntityData>(string entityDataId) where TEntityData : IEntityData where TEntity : IEntity<IEntityData, IEntitySavedData>
        {
            var entityData = _dataService.Load<TEntityData>(entityDataId);
            var entity = entityData.CreateEntity();

            _binder.Bind<TEntity>(entity, entityDataId);

            entity.Inject(_resolver);
            entity.Initialize();

            return entity;
        }

        public void DestroyEntity<TEntity>(string entityDataId) where TEntity : IEntity<IEntityData, IEntitySavedData>
        {
            var entity = _resolver.Resolve<TEntity>(entityDataId);

            entity.Terminate();

            _binder.Unbind<TEntity>(entity, entityDataId);
        }
    }
}