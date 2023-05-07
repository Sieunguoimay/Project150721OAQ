using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sieunguoimay.Serialization.Tools
{
#if UNITY_EDITOR
    [EditorWindowTitle(title = "AssetReferenceReplaceWindow")]
    public class AssetReferenceReplaceWindow : EditorWindow
    {
        private string _folder;
        private PreviewItem[] _previewItems;

        [MenuItem("Tools/Snm/AssetReferenceReplaceWindow")]
        public static void Open()
        {
            var window = GetWindow<AssetReferenceReplaceWindow>(false, "AssetReferenceReplaceWindow", true);
            window.Show();
        }

        private void OnGUI()
        {
            DrawReferenceReplace();
        }

        private void DrawReferenceReplace()
        {
            EditorGUILayout.BeginHorizontal();
            _folder = EditorGUILayout.TextField(new GUIContent("Folder"), _folder);
            if (GUILayout.Button("Use selected assetPath",GUILayout.Width(150)))
            {
                var newFolderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (AssetDatabase.IsValidFolder(newFolderPath))
                    _folder = newFolderPath;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var preview = GUILayout.Button("Preview");
            var replace = GUILayout.Button("Replace");
            EditorGUILayout.EndHorizontal();
            DrawPreviewItems();

            if (preview)
            {
                var previewItems = new List<PreviewItem>();
                TraverseAssets((serialized, it) =>
                {
                    previewItems.Add(new PreviewItem()
                    {
                        propertyName = it.propertyPath,
                        oldObject = it.objectReferenceValue,
                        newObject = FindAssetInFolder(it.objectReferenceValue)
                    });
                    return false;
                });
                _previewItems = previewItems.ToArray();
            }
            if (replace)
            {
                TraverseAssets((serialized, it) =>
                {

                    var found = FindAssetInFolder(it.objectReferenceValue);
                    if (found)
                    {
                        serialized.Update();
                        it.objectReferenceValue = found;
                        serialized.ApplyModifiedProperties();
                    }
                    return found != null;
                });
            }
        }
        private void DrawPreviewItems()
        {
            if (_previewItems == null) return;
            foreach (var item in _previewItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(new GUIContent(item.propertyName), item.oldObject, typeof(Object), true);
                EditorGUILayout.LabelField(new GUIContent("->"), GUILayout.Width(30));
                EditorGUILayout.ObjectField(GUIContent.none, item.newObject, typeof(Object), true);
                EditorGUILayout.EndHorizontal();
            }
        }
        private void TraverseAssets(Func<SerializedObject, SerializedProperty, bool> onFoundProperty)
        {
            var selected = Selection.objects;
            foreach (var asset in selected)
            {
                var changed = false;
                if (asset is GameObject go)
                {
                    var unityObjects = go.GetComponentsInChildren<Component>(true);
                    foreach (var unityObject in unityObjects)
                    {
                        changed = IterateSerializedProperties(unityObject, onFoundProperty);
                    }
                }
                else
                {
                    changed = IterateSerializedProperties(asset, onFoundProperty);
                }
                if (changed)
                {
                    AssetDatabase.SaveAssets();
                }
            }
        }
        private bool IterateSerializedProperties(Object asset, Func<SerializedObject, SerializedProperty, bool> onFoundProperty)
        {
            var serialized = new SerializedObject(asset);
            var it = serialized.GetIterator();
            var changed = false;
            while (it.Next(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (it.objectReferenceValue != null
                        && AssetDatabase.IsMainAsset(it.objectReferenceValue)
                        && it.objectReferenceValue != asset
                        && it.objectReferenceValue is not MonoScript)
                    {
                        changed = onFoundProperty?.Invoke(serialized, it) ?? false;
                    }
                }
            }
            return changed;
        }

        private Object FindAssetInFolder(Object asset)
        {
            var name = asset.name;
            var path = AssetDatabase.GetAssetPath(asset);
            var ext = Path.GetExtension(path);
            var founds = AssetDatabase.FindAssets($"{name}", new[] { _folder }).Select(f => AssetDatabase.GUIDToAssetPath(f));
            founds = founds.Where(f => Path.GetExtension(f).Equals(ext));
            return AssetDatabase.LoadAssetAtPath<Object>(founds.FirstOrDefault());
        }
        [Serializable]
        private class PreviewItem
        {
            public string propertyName;
            public Object oldObject;
            public Object newObject;
        }
    }
#endif
}