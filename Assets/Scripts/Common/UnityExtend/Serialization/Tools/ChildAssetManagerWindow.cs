#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Serialization.Tools
{
    [EditorWindowTitle(title = "Child Assets")]
    public class ChildAssetManagerWindow : EditorWindow
    {
        private Object _rootAsset;
        private Object[] _assets;
        private Object _renameObject;
        private Object _editObject;
        private string _name;
        private bool _showEditForAll;

        [MenuItem("Tools/Child Assets")]
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
            EditorGUILayout.ObjectField(MonoScript.FromScriptableObject(this), GetType(), false);
            GUI.enabled = true;

            if (Selection.activeObject == null) return;

            var target = Selection.activeObject;

            if (_currentTarget == null || _currentTarget != target)
            {
                _currentTarget = target;

                var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_currentTarget));
                _assets = allAssets.Where(AssetDatabase.IsSubAsset).ToArray();
                _rootAsset = allAssets.FirstOrDefault(a => !AssetDatabase.IsSubAsset(a));
                if (_rootAsset != _currentTarget)
                {
                    _editObject = _currentTarget;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(_rootAsset, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(new GUIContent("#", "Show Edit for all"), GUILayout.Width(20)))
            {
                _showEditForAll = !_showEditForAll;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            DrawSubAssets();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawPullAssetToSubAssetUI();
            DrawNewChildAssetMenu(AssetDatabase.GetAssetPath(_rootAsset), newAsset =>
            {
                RefreshCurrentAsset();
                Repaint();
                _name = newAsset.name;
                _renameObject = newAsset;
                GUI.FocusControl(null);
            });
            
            if (GUILayout.Button(new GUIContent("...","Open ScriptableObject window"), GUILayout.Width(20)))
            {
                ScriptableObjectCreatorWindow.Open();
            }

            EditorGUILayout.EndHorizontal();
            if (DrawAllSelectedAssets(!_includeActiveBaseAsset, true, () =>
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

        private void DrawSubAssets()
        {
            var alignment = GUI.skin.button.alignment;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            foreach (var asset in _assets)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(10, false);
                if (_renameObject != asset)
                {
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;

                    if (GUILayout.Button(EditorGUIUtility.ObjectContent(asset,typeof(Object)),EditorStyles.objectField, GUILayout.Height(18)))
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                        _editObject = _editObject != asset ? asset : null;
                    }


                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;

                    if (_editObject == asset || _showEditForAll)
                    {

                        if (GUILayout.Button(new GUIContent("R", "Rename"), GUILayout.Width(20)))
                        {
                            _name = asset.name;
                            _renameObject = asset;
                            GUI.FocusControl(null);
                        }

                        if (GUILayout.Button(
                                new GUIContent("D" + (_multiTargetDuplicateActive ? "++" : ""), "Duplicate"),
                                GUILayout.Width(_multiTargetDuplicateActive ? 35 : 20)))
                        {
                            if (_multiTargetDuplicateActive)
                            {
                                var selected = Selection.objects;
                                foreach (var a in selected)
                                {
                                    if (!ValidateAsset(a)) continue;
                                    AssetToSubAsset(_editObject, a);
                                }
                            }
                            else
                            {
                                DuplicateChildAsset(asset, _assets);

                                Debug.Log("Duplicate");
                                RefreshCurrentAsset();
                                Repaint();
                            }
                        }
                        if (GUILayout.Button(new GUIContent("<-", "Extract Child Asset"), GUILayout.Width(25)))
                        {
                            ExtractChildAsset(_editObject);
                            RefreshCurrentAsset();
                            Repaint();
                        }

                        var color = GUI.color;
                        GUI.color = Color.red;
                        if (GUILayout.Button(new GUIContent("X", "Delete"), GUILayout.Width(20)))
                        {
                            AssetDatabase.RemoveObjectFromAsset(asset);
                            AssetDatabase.SaveAssets();
                            RefreshCurrentAsset();
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
        }

        private void RefreshCurrentAsset()
        {
            _currentTarget = null;
            Selection.activeObject = _rootAsset;
        }

        private bool _includeActiveBaseAsset;
        private bool _multiTargetDuplicateActive;

        private void DrawMultiTargetAction()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var color = GUI.color;
            GUI.color = _includeActiveBaseAsset ? Color.cyan : color;
            if (GUILayout.Button("Include Active Base Asset"))
            {
                _includeActiveBaseAsset = !_includeActiveBaseAsset;
                _activeSelectedAsset = null;
            }

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

        private bool DrawAllSelectedAssets(bool excludeActiveBaseAsset, bool excludeChildAsset = true,
            Action onDrawPrefix = null)
        {
            _selectedAssetsFoldout = EditorGUILayout.Foldout(_selectedAssetsFoldout, "Other Selected", true);
            if (!_selectedAssetsFoldout) return false;

            var assets = Selection.objects;
            var active = Selection.activeObject;
            if (excludeActiveBaseAsset)
            {
                if (active != _activeSelectedAsset)
                {
                    _activeSelectedAsset = active;
                    _cachedSelectedParentAsset = AssetDatabase.IsSubAsset(active)
                        ? AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(active))
                        : active;
                }
            }
            else
            {
                _cachedSelectedParentAsset = AssetDatabase.IsSubAsset(active) ? active : null;
            }

            var count = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.BeginDisabledGroup(true);
            foreach (var a in assets)
            {
                if (!ValidateAsset(a, excludeChildAsset)) continue;
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

        private bool ValidateAsset(Object a, bool excludeChildAsset = true)
        {
            var childAsset = excludeChildAsset && AssetDatabase.IsSubAsset(a);
            return a != _cachedSelectedParentAsset && !childAsset;
        }

        private bool _pulling;
        private Object _pulledAsset;
        private Object _currentTarget;

        private void DrawPullAssetToSubAssetUI()
        {
            if (!_pulling)
            {
                if (GUILayout.Button(new GUIContent("->", "To SubAsset"),GUILayout.Width(25)))
                {
                    _pulling = true;
                }
            }
            else
            {
                GUILayout.Label(new GUIContent("Free asset", "Drag free asset in here"));
                _pulledAsset = EditorGUILayout.ObjectField(GUIContent.none, _pulledAsset, typeof(Object), false);

                if (GUILayout.Button(_pulledAsset != null ? new GUIContent("OK") : new GUIContent("X", "Collapse")))
                {
                    _pulling = false;
                    AssetToSubAsset(_pulledAsset, _rootAsset);
                    _pulledAsset = null;
                }
            }
        }

        private static void DrawNewChildAssetMenu(string path, Action<Object> created)
        {
            if (!GUILayout.Button(new GUIContent("+", "New child asset"))) return;
            var menu = new GenericMenu();
            var typesContainsAttribute = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t));
            foreach (var t in typesContainsAttribute)
            {
                menu.AddItem(new GUIContent(t.Name), false, () =>
                {
                    var newAsset = CreateAssetOfType(t, t.Name, path);
                    Debug.Log($"{t.Name}", newAsset);
                    created?.Invoke(newAsset);
                });
            }

            menu.ShowAsContext();
        }

        #region AssetManipulate

        private static void DuplicateChildAsset(Object asset, Object[] assets)
        {
            var newAsset = CreateAssetOfType(asset.GetType(), "", AssetDatabase.GetAssetPath(asset));
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
            if (freeAsset == null) return;
            if (!AssetDatabase.IsSubAsset(parent))
            {
                var child = CreateAssetOfType(freeAsset.GetType(), freeAsset.name, AssetDatabase.GetAssetPath(parent));
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


        public static Object CreateAssetOfType(Type type, string assetName, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            
            var instance = CreateInstance(type);
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
#endif