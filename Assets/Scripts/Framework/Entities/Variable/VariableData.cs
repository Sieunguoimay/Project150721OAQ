using Framework.Entities;
using UnityEngine;
using System;

namespace Framework.Entities.Variable
{
    public interface IVariableData : IEntityData
    {
    }

    public interface IVariableSavedData<TPrimitive> : IEntitySavedData
    {
        TPrimitive Value { get; }
        void SetValue(TPrimitive value);
    }

    [CreateAssetMenu(menuName = "Entity/VariableData")]
    public class VariableData<TPrimitive> : EntityAsset<IVariable<TPrimitive>>, IVariableData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Variable<TPrimitive>(this, new VariableSavedData<TPrimitive>(Id));
        }
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