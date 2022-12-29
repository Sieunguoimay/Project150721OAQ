using UnityEngine;

namespace Framework.Entities
{
    public interface IEntityView<out TEntity>
    {
        TEntity Entity { get; }
    }

    public interface IManualView
    {
        void Setup(object entity);
        void TearDown();
    }

    public abstract class BaseEntityManualView<TEntity> : MonoBehaviour, IEntityView<TEntity>, IManualView
        where TEntity : IEntity<IEntityData, IEntitySavedData>
    {
        public TEntity Entity { get; private set; }

        public void Setup(object entity)
        {
            Entity = (TEntity) entity;
            OnSetup();
        }

        public void TearDown()
        {
            OnTearDown();
            Entity = default;
        }

        protected virtual void OnSetup()
        {
        }

        protected virtual void OnTearDown()
        {
        }
    }
}