using UnityEngine;
using System;

namespace Framework.Entities.Variable
{
    public interface IVariableEntityData<out TPrimitive> : IEntityData
    {
        TPrimitive InitialValue { get; }
    }

    public interface IVariableSavedData<TPrimitive> : IEntitySavedData
    {
        TPrimitive Value { get; }
        void SetValue(TPrimitive value);
    }

    [CreateAssetMenu(menuName = "Entity/VariableData")]
    public class VariableEntityData<TPrimitive> : EntityAsset<IVariableEntity<TPrimitive>>, IVariableEntityData<TPrimitive>
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            return new VariableEntity<TPrimitive>(this, null);
        }

        [field: SerializeField] public TPrimitive InitialValue { get; private set; }
    }

    [Serializable]
    public class VariableSavedData<TPrimitive> : BaseEntitySavedData<IVariableEntityData<TPrimitive>>,
        IVariableSavedData<TPrimitive>
    {
        public VariableSavedData(IVariableEntityData<TPrimitive> entityData) : base(entityData)
        {
        }

        protected override void InitializeDefaultData(IVariableEntityData<TPrimitive> entityData)
        {
            base.InitializeDefaultData(entityData);
            Value = entityData.InitialValue;
        }

        [field: SerializeField] public TPrimitive Value { get; private set; }

        public void SetValue(TPrimitive value)
        {
            Value = value;
            Save();
        }
    }
}