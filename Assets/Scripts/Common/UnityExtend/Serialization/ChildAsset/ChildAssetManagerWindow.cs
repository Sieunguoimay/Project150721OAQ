using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Serialization.ChildAsset
{
    public class ChildAssetManagerWindow : EditorWindow
    {
        public static void Open()
        {
            var window = GetWindow<ChildAssetManagerWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            if (Selection.activeObject != null)
            {
                var target = Selection.activeObject;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(target, typeof(Object),false);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}