using Framework.Resolver;
using Framework.Services;
using Framework.Services.Data;

namespace Framework.Entities
{
    public interface IEntityLoader : IInjectable
    {
        IEntity<IEntityData, IEntitySavedData> CreateEntity(string entityDataId);

        void DestroyEntity(IEntity<IEntityData, IEntitySavedData> entity);
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

        public IEntity<IEntityData, IEntitySavedData> CreateEntity(string entityDataId)
        {
            var entityAsset = _dataService.Load<IEntityData>(entityDataId);
            var entity = entityAsset.CreateEntity();

            _binder.Bind(entityAsset.GetEntityType(), entityDataId, entity);

            entity.Inject(_resolver);
            entity.Initialize();
            return entity;
        }

        public void DestroyEntity(IEntity<IEntityData, IEntitySavedData> entity)
        {
            entity.Terminate();

            _binder.Unbind(entity.Data.GetEntityType(), entity.Data.Id);
        }
    }
}