using System;
using Framework.Entities;

namespace Framework.Entities.Variable.Boolean
{
    public interface IBoolean : IVariableEntity<bool>
    {
    }

    public class Boolean : VariableEntity<bool>, IBoolean
    {
        public Boolean(IBooleanData data, IBooleanSavedData savedData) : base(data, savedData)
        {
        }
    }
}