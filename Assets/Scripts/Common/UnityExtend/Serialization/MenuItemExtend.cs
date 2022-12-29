using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Serialization
{
    public static class MenuItemExtend
    {
        [MenuItem("CONTEXT/ScriptableObject/Ping me!")]
        private static void PingMe(MenuCommand m)
        {
            var o = m.context as ScriptableObject;
            EditorGUIUtility.PingObject(o);
        }
    }
}