using Framework.Entities;
using UnityEngine;
using System;

namespace Framework.Entities.Variable.Boolean
{
    public interface IBooleanData : IVariableData
    {
    }

    public interface IBooleanSavedData : IVariableSavedData<bool>
    {
    }

    [CreateAssetMenu(menuName = "Entity/BooleanData")]
    public class BooleanData : VariableData<bool>, IBooleanData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Boolean(this, new BooleanSavedData(Id));
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
        public BooleanSavedData(string id) : base(id)
        {
        }
    }
}