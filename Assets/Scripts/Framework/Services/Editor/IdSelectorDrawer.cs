using System.Linq;
using Common.UnityExtend.Attribute;
using UnityEditor;
using UnityEngine;

namespace Framework.Services.Editor
{
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(IdSelectorAttribute))]
    public class IdSelectorDrawer : StringSelectorDrawer
    {
        protected override string[] GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            return IdsHelper.GetIds((objectSelector as IdSelectorAttribute)?.TypeConstraint).ToArray();
        }
    }
#endif
}