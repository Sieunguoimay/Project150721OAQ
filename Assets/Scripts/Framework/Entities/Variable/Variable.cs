using System;
using Framework.Entities;

namespace Framework.Entities.Variable
{
    public interface IVariable<TPrimitive> : IEntity<IVariableData, IVariableSavedData<TPrimitive>>
    {
        void SetValue(TPrimitive value);
        TPrimitive Value { get; }
        event Action<object, EventArgs> ValueChanged;
    }

    public class Variable<TPrimitive> : BaseEntity<IVariableData, IVariableSavedData<TPrimitive>>, IVariable<TPrimitive>
    {
        public Variable(IVariableData data, IVariableSavedData<TPrimitive> savedData) : base(data, savedData)
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