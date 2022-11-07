using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using UnityEditor;

namespace Framework.Services.Data.Editor
{
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(IdSelectorAttribute))]
    public class IdSelectorDrawer : StringSelectorDrawer
    {
        protected override IEnumerable<string> GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            return IdsHelper.GetIds((objectSelector as IdSelectorAttribute)?.TypeConstraint);
        }
    }
#endif
}