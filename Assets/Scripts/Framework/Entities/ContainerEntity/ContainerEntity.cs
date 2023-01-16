using System.Collections.Generic;
using System.Linq;
using Framework.Resolver;
using Framework.Services.Data;
using UnityEngine;

namespace Framework.Entities.ContainerEntity
{
    public interface IContainerEntity<out TData, out TSavedData> : IEntity<TData, TSavedData>
        where TData : IContainerEntityData where TSavedData : IContainerEntitySavedData
    {
        IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> Components { get; }
        IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> Children { get; }
        void SetupInternal(IEntity<IEntityData, IEntitySavedData>[] components, IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> loadAlongEntities);
    }

    public class ContainerEntity<TData, TSavedData> : BaseEntity<TData, TSavedData>, IContainerEntity<TData, TSavedData>
        where TData : IContainerEntityData where TSavedData : IContainerEntitySavedData
    {
        protected ContainerEntity(TData data, TSavedData savedData) : base(data, savedData)
        {
        }


        public IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> Components { get; private set; }
        public IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> Children { get; private set; }
        
        private IEntityLoader _entityLoader;

        public void SetupInternal(IEntity<IEntityData, IEntitySavedData>[] components, IReadOnlyList<IEntity<IEntityData, IEntitySavedData>> loadAlongEntities)
        {
            Components = components;
            Children = loadAlongEntities;
        }

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            foreach (var component in Components)
            {
                component.Inject(resolver);
            }

            _entityLoader = resolver.Resolve<IEntityLoader>();
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach (var component in Components)
            {
                component.Initialize();
            }
        }

        public override void Terminate()
        {
            base.Terminate();
            foreach (var component in Components)
            {
                component.Terminate();
            }

            foreach (var la in Children)
            {
                _entityLoader.DestroyEntity(la);
            }
        }

        protected TEntity GetComponentEntity<TEntity>(string id) where TEntity : class
        {
            var result = Components.FirstOrDefault(c => c.Data.Id.Equals(id) && c is TEntity) as TEntity;
            if (result == null)
            {
                Debug.LogError($"Component entity not found {id}", Data as DataAsset);
            }

            return result;
        }
    }
}