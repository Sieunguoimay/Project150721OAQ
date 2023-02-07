using System;

namespace Framework.Entities.Variable
{
    public interface IVariableEntity<TPrimitive> : IEntity<IVariableEntityData<TPrimitive>, IVariableSavedData<TPrimitive>>, IVariable<TPrimitive>
    {
    }

    public class VariableEntity<TPrimitive> : BaseEntity<IVariableEntityData<TPrimitive>, IVariableSavedData<TPrimitive>>, IVariableEntity<TPrimitive>
    {
        public VariableEntity(IVariableEntityData<TPrimitive> data, IVariableSavedData<TPrimitive> savedData) : base(data, savedData)
        {
        }

        public void SetValue(TPrimitive value)
        {
            SavedData.SetValue(value);
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public TPrimitive Value => SavedData.Value;
        public event Action<object, EventArgs> ValueChanged;
    }
}