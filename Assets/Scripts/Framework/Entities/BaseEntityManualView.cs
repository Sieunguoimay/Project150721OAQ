using UnityEngine;

namespace Framework.Entities
{
    public interface IEntityView<out TEntity>
    {
        TEntity Entity { get; }
    }

    public abstract class BaseEntityManualView<TEntity> : MonoBehaviour, IEntityView<TEntity>
        where TEntity : IEntity<IEntityData, IEntitySavedData>
    {
        public TEntity Entity { get; private set; }

        public void Setup(TEntity entity)
        {
            Entity = entity;
            OnSetup();
        }

        public void TearDown()
        {
            OnTearDown();
            Entity = default;
        }

        private void OnSetup()
        {
        }

        private void OnTearDown()
        {
        }
    }
}