using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Entities.Variable.Boolean
{
    public class BooleanEntityManualView : VariableEntityManualView<bool>
    {
        [SerializeField] private UnityEvent onTrue;
        [SerializeField] private UnityEvent onFalse;
        
        protected override void OnValueChanged(object arg1, EventArgs arg2)
        {
            base.OnValueChanged(arg1, arg2);
            if (Entity.Value)
            {
                onTrue?.Invoke();
            }
            else
            {
                onFalse?.Invoke();
            }
        }
    }
}