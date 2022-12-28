using System;
using Framework.Entities;

namespace Framework.Entities.Variable.Boolean
{
    public interface IBoolean : IVariable<bool>
    {
    }

    public class Boolean : Variable<bool>, IBoolean
    {
        public Boolean(IBooleanData data, IBooleanSavedData savedData) : base(data, savedData)
        {
        }
    }
}