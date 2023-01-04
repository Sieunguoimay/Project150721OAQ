using System;
using Common.UnityExtend.Attribute;

namespace Framework.Services.Data
{
    public class DataAssetIdSelectorAttribute : StringSelectorAttribute
    {
        public Type TypeConstraint { get; }

        public DataAssetIdSelectorAttribute() : base("")
        {
        }

        public DataAssetIdSelectorAttribute(Type typeConstraint) : base("")
        {
            TypeConstraint = typeConstraint;
        }
    }
}