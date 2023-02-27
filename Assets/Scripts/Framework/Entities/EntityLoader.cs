using System.Collections.Generic;
using Framework.Resolver;
using Framework.Services;
using Framework.Services.Data;
using UnityEngine;

namespace Framework.Entities
{
    public interface IEntityLoader
    {
        /// <summary>
        /// Better call CreateEntity() inside Inject() Method
        /// </summary>
        /// <param name="entityDataId"></param>
        /// <returns></returns>
        IEntity<IEntityData, IEntitySavedData> CreateEntity(string entityDataId);

        void DestroyEntity(IEntity<IEntityData, IEntitySavedData> entity);
    }

    //Only for entity
    public class EntityLoader : IEntityLoader, IInjectable
    {
        private IDataService _dataService;
        private IBinder _binder;
        private ISavedDataService _savedDataService;

        private List<IEntity<IEntityData, IEntitySavedData>> _entitiesTobeSetup;

        public void Inject(IResolver resolver)
        {
            _dataService = resolver.Resolve<IDataService>();
            _binder = resolver.Resolve<IBinder>();
            _savedDataService = resolver.Resolve<ISavedDataService>();
            _entitiesTobeSetup = new List<IEntity<IEntityData, IEntitySavedData>>();
        }

        public IEntity<IEntityData, IEntitySavedData> CreateEntity(string entityDataId)
        {
            var entityAsset = _dataService.Load<IEntityData>(entityDataId);
            var entity = entityAsset.CreateEntity(this);
            entity.Bind(_binder);
            entity.SavedData?.Load(_savedDataService);
            return entity;
        }

        public void DestroyEntity(IEntity<IEntityData, IEntitySavedData> entity)
        {
            entity.TearDownDependencies();
            entity.Unbind(_binder);
        }
    }
}