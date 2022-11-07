using System;
using Common.UnityExtend.Attribute;

namespace Framework.Services.Data
{
    public class IdSelectorAttribute : StringSelectorAttribute
    {
        public Type TypeConstraint { get; }

        public IdSelectorAttribute() : base("")
        {
        }

        public IdSelectorAttribute(Type typeConstraint) : base("")
        {
            TypeConstraint = typeConstraint;
        }
    }
}