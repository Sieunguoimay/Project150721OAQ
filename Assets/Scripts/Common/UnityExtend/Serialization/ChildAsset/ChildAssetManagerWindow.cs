﻿using System;
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
        private bool _showEditForAll;

        public static void Open()
        {
            var window = GetWindow<ChildAssetManagerWindow>("Child Assets");
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

            if (_target == null || _target != target)
            {
                _target = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(target));

                _assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target))
                    .Where(AssetDatabase.IsSubAsset).ToArray();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(_target, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("#", GUILayout.Width(20)))
            {
                _showEditForAll = !_showEditForAll;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            var alignment = GUI.skin.button.alignment;
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
                        _editObject = _editObject != asset ? asset : null;
                    }

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;

                    if (_editObject == asset || _showEditForAll)
                    {
                        if (GUILayout.Button("<-", GUILayout.Width(25)))
                        {
                            ExtractChildAsset(_editObject);
                            _target = null;
                            Repaint();
                        }

                        if (GUILayout.Button("R", GUILayout.Width(20)))
                        {
                            _name = asset.name;
                            _renameObject = asset;
                            GUI.FocusControl(null);
                        }

                        if (GUILayout.Button("D" + (_multiTargetDuplicateActive ? "++" : ""), GUILayout.Width(_multiTargetDuplicateActive ? 35 : 20)))
                        {
                            if (_multiTargetDuplicateActive)
                            {
                                var selected = Selection.objects;
                                foreach (var a in selected)
                                {
                                    if (!ShouldSkipAsset(a)) continue;
                                    AssetToSubAsset(_editObject, a);
                                }
                            }
                            else
                            {
                                DuplicateChildAsset(asset, _assets);

                                Debug.Log("Duplicate");
                                _target = null;
                                Repaint();
                            }
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

            GUI.skin.button.alignment = alignment;

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawPullAssetToSubAssetUI();
            DrawNewChildAssetMenu(AssetDatabase.GetAssetPath(_target), (newAsset) =>
            {
                _target = null;
                Repaint();
                _name = newAsset.name;
                _renameObject = newAsset;
                GUI.FocusControl(null);
            });

            EditorGUILayout.EndHorizontal();
            if (DrawAllSelectedAssets(true, true, () =>
            {
                if (_multiTargetDuplicateActive)
                {
                    EditorGUILayout.LabelField("D->", GUILayout.Width(25));
                }
            }))
            {
                DrawMultiTargetAction();
            }
            else
            {
                _multiTargetDuplicateActive = false;
            }
        }

        private bool _multiTargetDuplicateActive;

        private void DrawMultiTargetAction()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var color = GUI.color;
            GUI.color = _multiTargetDuplicateActive ? Color.cyan : color;
            if (GUILayout.Button("Duplicate to Others"))
            {
                _multiTargetDuplicateActive = !_multiTargetDuplicateActive;
            }

            GUI.color = color;

            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        private bool _selectedAssetsFoldout;
        private Object _activeSelectedAsset;
        private Object _cachedSelectedParentAsset;

        private bool DrawAllSelectedAssets(bool excludeActive, bool excludeChildAsset = true, Action onDrawPrefix = null)
        {
            _selectedAssetsFoldout = EditorGUILayout.Foldout(_selectedAssetsFoldout, "Other Selected", true);
            if (!_selectedAssetsFoldout) return false;

            var assets = Selection.objects;
            var active = Selection.activeObject;
            if (excludeActive)
            {
                if (active != _activeSelectedAsset)
                {
                    _activeSelectedAsset = active;
                    _cachedSelectedParentAsset = AssetDatabase.IsSubAsset(active) ? AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(active)) : active;
                }
            }
            else
            {
                _cachedSelectedParentAsset = active;
            }

            var count = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.BeginDisabledGroup(true);
            foreach (var a in assets)
            {
                if (!ShouldSkipAsset(a, excludeChildAsset)) continue;
                EditorGUILayout.BeginHorizontal();
                onDrawPrefix?.Invoke();
                EditorGUILayout.ObjectField(a, typeof(Object), false);
                EditorGUILayout.EndHorizontal();
                count++;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            return count > 0;
        }

        private bool ShouldSkipAsset(Object a, bool excludeChildAsset = true)
        {
            var exclude = excludeChildAsset && AssetDatabase.IsSubAsset(a);
            return a != _cachedSelectedParentAsset && !exclude;
        }

        private bool _pulling;
        private Object _pulledAsset;

        private void DrawPullAssetToSubAssetUI()
        {
            if (!_pulling)
            {
                if (GUILayout.Button("->"))
                {
                    _pulling = true;
                }
            }
            else
            {
                _pulledAsset = EditorGUILayout.ObjectField(GUIContent.none, _pulledAsset, typeof(Object), false);

                if (GUILayout.Button("OK"))
                {
                    _pulling = false;
                    AssetToSubAsset(_pulledAsset, _target);
                    _pulledAsset = null;
                }
            }
        }

        private static void DrawNewChildAssetMenu(string path, Action<Object> created)
        {
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
        }

        #region AssetManipulate

        private static void DuplicateChildAsset(Object asset, Object[] assets)
        {
            var newAsset = CreateChildAsset(asset.GetType(), "", AssetDatabase.GetAssetPath(asset));
            EditorUtility.CopySerialized(asset, newAsset);
            var n = $"{asset.name}";
            var count = 0;
            while (assets.Any(a => a.name.Equals(n)))
            {
                n = $"{asset.name}_{++count}";
                if (count < 1000) continue;
                Debug.LogError("Err too many same name");
                return;
            }

            newAsset.name = n;
            EditorUtility.SetDirty(newAsset);
            AssetDatabase.SaveAssets();
        }

        private static void ExtractChildAsset(Object childAsset)
        {
            var parentPath = AssetDatabase.GetAssetPath(childAsset);
            var dir = Path.GetDirectoryName(parentPath) ?? string.Empty;
            var path = Path.Combine(dir, childAsset.name + ".asset");
            var count = 0;

            while (!string.IsNullOrEmpty(AssetDatabase.ValidateMoveAsset(parentPath, path)))
            {
                path = Path.Combine(dir, $"{childAsset.name}_{++count}.asset");
                if (count <= 1000) continue;
                Debug.LogError($"Shit>< {count} assets of the same name is not good!!");
                return;
            }

            // AssetDatabase.RemoveObjectFromAsset(_editObject);
            var newAsset = CreateInstance(childAsset.GetType());
            EditorUtility.CopySerialized(childAsset, newAsset);
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            Debug.Log(path, childAsset);
        }

        private static void AssetToSubAsset(Object freeAsset, Object parent)
        {
            if (!AssetDatabase.IsSubAsset(parent))
            {
                var child = CreateChildAsset(freeAsset.GetType(), freeAsset.name, AssetDatabase.GetAssetPath(parent));
                EditorUtility.CopySerialized(freeAsset, child);
                // AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(freeAsset));
                AssetDatabase.SaveAssets();
                Debug.Log(child.name + " -> " + parent.name, child);
            }
            else
            {
                Debug.Log("Failed");
            }
        }

        public static void ExtractFromMonoBehaviourAsset(Object subAsset, string destinationPath)
        {
            var assetPath = AssetDatabase.GetAssetPath(subAsset);
            AssetDatabase.ExtractAsset(subAsset, destinationPath);
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
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

        #endregion
    }
}