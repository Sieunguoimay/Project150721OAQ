using Framework.Entities;
using UnityEngine;
using System;

namespace Framework.Entities.Variable.Boolean
{
    public interface IBooleanData : IVariableEntityData<bool>
    {
    }

    public interface IBooleanSavedData : IVariableSavedData<bool>
    {
    }

    public class BooleanData : VariableEntityData<bool>, IBooleanData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            return new Boolean(this, new BooleanSavedData(this));
        }
#if UNITY_EDITOR
        [ContextMenu("Test")]
        void Test()
        {
            DebugEntity.SetValue(!DebugEntity.Value);
        }
#endif
    }

    [Serializable]
    public class BooleanSavedData : VariableSavedData<bool>, IBooleanSavedData
    {
        public BooleanSavedData(IVariableEntityData<bool> entityData) : base(entityData)
        {
        }
    }
}