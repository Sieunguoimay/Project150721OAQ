using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Serialization.ChildAsset
{
    public class ChildAssetManagerWindow : EditorWindow
    {
        private Object _target;
        private Object[] _assets;
        private Object _renameObject;
        private Object _editObject;
        private string _name;

        public static void Open()
        {
            var window = GetWindow<ChildAssetManagerWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }


        private void OnSelectionChanged()
        {
            _renameObject = null;
            Repaint();
        }

        private void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(this), GetType(), false);
            GUI.enabled = true;

            if (Selection.activeObject == null) return;

            var target = Selection.activeObject;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(target, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (_target == null || _target != target)
            {
                _target = target;

                _assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target))
                    .Where(AssetDatabase.IsSubAsset).ToArray();
            }

            EditorGUILayout.BeginVertical();
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            foreach (var asset in _assets)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(10, false);
                if (_renameObject != asset)
                {
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if (GUILayout.Button($"{asset.name} ({asset.GetType().Name})"))
                    {
                        EditorGUIUtility.PingObject(asset);
                        _editObject = asset;
                    }

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;

                    if (_editObject == asset)
                    {
                        if (GUILayout.Button("R", GUILayout.Width(20)))
                        {
                            _name = asset.name;
                            _renameObject = asset;
                            GUI.FocusControl(null);
                        }

                        if (GUILayout.Button("D", GUILayout.Width(20)))
                        {
                            Debug.Log("Duplicate");
                            var newAsset = CreateChildAsset(asset.GetType(), "", AssetDatabase.GetAssetPath(_target));
                            EditorUtility.CopySerialized(asset, newAsset);
                            newAsset.name = $"{asset.name}_cloned";
                            EditorUtility.SetDirty(newAsset);
                            AssetDatabase.SaveAssets();
                            _target = null;
                            Repaint();
                        }

                        var color = GUI.color;
                        GUI.color = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            AssetDatabase.RemoveObjectFromAsset(asset);
                            AssetDatabase.SaveAssets();
                            _target = null;
                            Repaint();
                        }

                        GUI.color = color;
                    }
                }
                else
                {
                    _name = EditorGUILayout.TextField(_name);
                    if (GUILayout.Button("OK", GUILayout.Width(45)))
                    {
                        if (!_name.Equals(asset.name) && !string.IsNullOrEmpty(_name))
                        {
                            asset.name = _name;
                            EditorUtility.SetDirty(asset);
                            AssetDatabase.SaveAssets();

                            Debug.Log(asset.name);
                        }

                        _renameObject = null;
                        GUI.FocusControl(null);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            DrawNewChildAssetMenu(AssetDatabase.GetAssetPath(_target), (newAsset) =>
            {
                _target = null;
                Repaint();
                _name = newAsset.name;
                _renameObject = newAsset;
                GUI.FocusControl(null);
            });
        }

        private void DrawFromAssetToSubAsset()
        {
            
        }

        private static void DrawNewChildAssetMenu(string path, Action<Object> created)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+"))
            {
                var menu = new GenericMenu();
                var typesContainsAttribute = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.GetCustomAttributes<CreateAssetMenuAttribute>(true).Any());
                foreach (var t in typesContainsAttribute)
                {
                    var att = t.GetCustomAttribute<CreateAssetMenuAttribute>(true);
                    var menuItem = string.IsNullOrEmpty(att.menuName) ? t.Name : att.menuName;
                    var assetName = string.IsNullOrEmpty(att.fileName) ? t.Name : att.fileName;
                    menu.AddItem(new GUIContent(menuItem), false, () =>
                    {
                        var newAsset = CreateChildAsset(t, assetName, path);
                        Debug.Log($"{menuItem} {assetName}", newAsset);
                        created?.Invoke(newAsset);
                    });
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static Object CreateChildAsset(Type type, string assetName, string path)
        {
            var instance = ScriptableObject.CreateInstance(type);
            instance.name = assetName;
            if (AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateAsset(instance, Path.Combine(path, assetName + ".asset"));
            }
            else
            {
                AssetDatabase.AddObjectToAsset(instance, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return instance;
        }
    }
}