using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Rendering
{
    public class SkinnedMeshBonesModifier : MonoBehaviour
    {
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SkinnedMeshBonesModifier))]
    public class SkinnedMeshBonesModifyBehaviourEditor : Editor
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is SkinnedMeshBonesModifier test && _skinnedMeshRenderer == null)
            {
                _skinnedMeshRenderer = test.GetComponent<SkinnedMeshRenderer>();
            }

            if (_skinnedMeshRenderer == null) return;
            Transform changed = null;
            var index = -1;
            for (var i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                var b = _skinnedMeshRenderer.bones[i];
                var newBone = (Transform) DrawObjectField(b, typeof(Transform), "" + i);
                if (b != newBone)
                {
                    changed = newBone;
                    index = i;
                    break;
                }
            }

            if (changed)
            {
                var bones = _skinnedMeshRenderer.bones;
                bones[index] = changed;
                _skinnedMeshRenderer.bones = bones;
            }

            var boneArr = _skinnedMeshRenderer.bones;
            if (DrawArrayModifiers(ref boneArr))
            {
                _skinnedMeshRenderer.bones = boneArr;
            }
        }

        private Object DrawObjectField(Object o, Type type, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(20 + 10 * Mathf.Max(0, label.Length - 2)));
            var newObject = EditorGUILayout.ObjectField(o, type, true);
            EditorGUILayout.EndHorizontal();
            return newObject;
        }

        private static bool DrawArrayModifiers<T>(ref T[] arr)
        {
            EditorGUILayout.BeginHorizontal();
            var modified = false;
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                Array.Resize(ref arr, arr.Length + 1);
                modified = true;
            }

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                Array.Resize(ref arr, arr.Length - 1);
                modified = true;
            }

            EditorGUILayout.EndHorizontal();
            return modified;
        }
    }
#endif
}