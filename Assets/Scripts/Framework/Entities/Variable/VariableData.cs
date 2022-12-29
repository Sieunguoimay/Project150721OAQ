using Framework.Entities;
using UnityEngine;
using System;

namespace Framework.Entities.Variable
{
    public interface IVariableData<out TPrimitive> : IEntityData
    {
        TPrimitive InitialValue { get; }
    }

    public interface IVariableSavedData<TPrimitive> : IEntitySavedData
    {
        TPrimitive Value { get; }
        void SetValue(TPrimitive value);
    }

    [CreateAssetMenu(menuName = "Entity/VariableData")]
    public class VariableData<TPrimitive> : EntityAsset<IVariable<TPrimitive>>, IVariableData<TPrimitive>
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Variable<TPrimitive>(this, new VariableSavedData<TPrimitive>(Id));
        }

        [field:SerializeField]public TPrimitive InitialValue { get; private set; }
    }
    
    [Serializable]
    public class VariableSavedData<TPrimitive> : BaseEntitySavedData, IVariableSavedData<TPrimitive>
    {
        public VariableSavedData(string id) : base(id)
        {
        }

        public TPrimitive Value { get; private set; }
        public void SetValue(TPrimitive value)
        {
            Value = value;
            Save();
        }
    }
}