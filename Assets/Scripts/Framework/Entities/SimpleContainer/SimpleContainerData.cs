using Framework.Entities;
using UnityEngine;
using System;
using Framework.Entities.ContainerEntity;

namespace Framework.Entities.SimpleContainer
{
    public interface ISimpleContainerData : IContainerEntityData
    {
    }

    public interface ISimpleContainerSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/SimpleContainerData")]
    public class SimpleContainerData : ContainerEntityData<ISimpleContainer>, ISimpleContainerData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new SimpleContainer(this, new SimpleContainerSavedData(this));
        }
    }

    [Serializable]
    public class SimpleContainerSavedData : ContainerEntitySavedData<ISimpleContainerData>, ISimpleContainerSavedData
    {
        public SimpleContainerSavedData(ISimpleContainerData data) : base(data)
        {
        }
    }
}