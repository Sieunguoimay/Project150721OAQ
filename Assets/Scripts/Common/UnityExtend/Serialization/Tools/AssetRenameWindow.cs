﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Serialization.Tools
{
#if UNITY_EDITOR
    public class AssetRenameWindow : EditorWindow
    {
        private string _param1Prefix;
        private string _param1Suffix;
        private string _param1Replace;
        private string _param2Replace;
        private string _propertyName;

        private readonly NoParamsModification _toLowerCaseAndUnderscore = new(
            "To Lowercase & Underscore",
            s =>
            {
                var pattern = new Regex(@"(?<!^)(?=[A-Z])|[^A-Za-z0-9]+");
                var result = pattern.Replace(s, "_").Trim('_').ToLower();
                return Regex.Replace(result, @"(?<=[A-Za-z])_(?=[A-Za-z])", "_");
            });

        private readonly TwoParamsModification _prefixModification = new(
            "Prefix",
            s => $"{s.Item2}{s.Item1}"
        );

        private readonly TwoParamsModification _suffixModification = new(
            "Suffix", s => $"{s.Item1}{s.Item2}"
        );

        private readonly TwoParamsModification _replaceModification = new("Replace", s =>
        {
            var originStr = s.Item1;
            var key = s.Item2;
            var newKey = s.Item3;
            return originStr.Replace(key, newKey);
        });

        private string[] _referencePrefixes =
        {
            "prefix",
        };

        private string[] _assetPrefixes =
        {
            "target_prefix",
        };

        private int _tab = 0;

        [MenuItem("Tools/Rename Assets")]
        public static void Open()
        {
            var window = GetWindow<AssetRenameWindow>(false, "Rename Assets", true);
            window.Show();
        }

        private void OnGUI()
        {
            DrawTabs();

            switch (_tab)
            {
                case 0:
                    Draw2ParamsModifier(ModifyNameOfSelected, _prefixModification, ref _param1Prefix, false);
                    Draw2ParamsModifier(ModifyNameOfSelected, _suffixModification, ref _param1Suffix, false);
                    DrawNameModifier2Params(ModifyNameOfSelected, _replaceModification, ref _param1Replace,
                        ref _param2Replace);
                    DrawNoParamsModifier(ModifyNameOfSelected, _toLowerCaseAndUnderscore);
                    break;
                case 1:
                    {
                        Draw2ParamsModifier(ModifyStringFieldOfReferenced, _prefixModification, ref _param1Prefix, false);
                        Draw2ParamsModifier(ModifyStringFieldOfReferenced, _suffixModification, ref _param1Suffix, false);
                        DrawNameModifier2Params(ModifyStringFieldOfReferenced, _replaceModification, ref _param1Replace,
                            ref _param2Replace);
                        DrawNoParamsModifier(ModifyStringFieldOfReferenced, _toLowerCaseAndUnderscore);

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Replace Tag"))
                        {
                            ChangeTag(_param1Replace, _param2Replace);
                        }

                        if (GUILayout.Button("Log Fields"))
                        {
                            LogFields();
                        }

                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                case 2:
                    Draw2ParamsModifier(RenamePrefabs, _prefixModification, ref _param1Prefix, false);
                    Draw2ParamsModifier(RenamePrefabs, _suffixModification, ref _param1Suffix, false);
                    DrawNameModifier2Params(RenamePrefabs, _replaceModification, ref _param1Replace,
                        ref _param2Replace);
                    break;
                case 3:
                    DrawRegexRename();
                    break;
            }

            DrawFooter();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Script"))
            {
                EditorGUIUtility.PingObject(MonoScript.FromScriptableObject(this));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            var prevColor = GUI.color;

            GUI.color = _tab == 0 ? Color.cyan : Color.white;
            if (GUILayout.Button("Rename selected"))
            {
                _tab = 0;
            }

            GUI.color = _tab == 1 ? Color.cyan : Color.white;
            if (GUILayout.Button("Rename fields of Asset"))
            {
                _tab = 1;
            }

            GUI.color = _tab == 2 ? Color.cyan : Color.white;
            if (GUILayout.Button("Rename fields of Prefabs"))
            {
                _tab = 2;
            }

            GUI.color = _tab == 3 ? Color.cyan : Color.white;
            if (GUILayout.Button("Replace References"))
            {
                _tab = 3;
            }

            GUI.color = prevColor;
            EditorGUILayout.EndHorizontal();
        }

        private string _regexPattern;
        private string _newString;
        private void DrawRegexRename()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Regex", GUILayout.Width(50));
            _regexPattern = EditorGUILayout.TextField(_regexPattern);
            EditorGUILayout.LabelField("->", GUILayout.Width(20));
            _newString = EditorGUILayout.TextField(_newString);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Rename"))
            {
                foreach (var item in Selection.objects)
                {
                    if (MatchPattern(item.name))
                    {
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item), _newString);
                        Debug.Log("match " + item.name);
                    }
                }
                AssetDatabase.SaveAssets();
            }
        }
        private bool MatchPattern(string name)
        {
            return Regex.IsMatch(name, _regexPattern, RegexOptions.IgnoreCase);
        }

        private class NoParamsModification
        {
            public readonly Func<string, string> Modify;
            public readonly string Label;

            public NoParamsModification(string l, Func<string, string> m)
            {
                Label = l;
                Modify = m;
            }
        }

        private static void DrawNoParamsModifier(Action<Func<string, string>> modifier,
            NoParamsModification modification)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(modification.Label))
            {
                modifier?.Invoke(s => modification.Modify?.Invoke(s));
            }

            EditorGUILayout.EndHorizontal();
        }

        private class TwoParamsModification
        {
            public readonly string Label;
            public readonly Func<(string, string, string), string> Modify;

            public TwoParamsModification(string l, Func<(string, string, string), string> m)
            {
                Label = l;
                Modify = m;
            }
        }

        private void Draw2ParamsModifier(Action<Func<string, string>> modifier, TwoParamsModification modification,
            ref string param1,
            bool useParam2 = true)
        {
            EditorGUILayout.BeginHorizontal();

            param1 = EditorGUILayout.TextField(modification.Label, param1);
            if (useParam2)
            {
                _param2Replace = EditorGUILayout.TextField(_param2Replace);
            }

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(param1))
                {
                    var p1 = param1;
                    modifier?.Invoke(s => modification.Modify?.Invoke((s, p1, _param2Replace)));
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNameModifier2Params(Action<Func<string, string>> modifier, TwoParamsModification modification,
            ref string param1,
            ref string param2)
        {
            EditorGUILayout.BeginHorizontal();

            param1 = EditorGUILayout.TextField(modification.Label, param1);
            param2 = EditorGUILayout.TextField(param2);

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(param1))
                {
                    var p1 = param1;
                    var p2 = param2;
                    modifier?.Invoke(s => modification.Modify?.Invoke((s, p1, p2)));
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void RenamePrefabs(Func<string, string> modify)
        {
            foreach (var o in Selection.objects)
            {
                if (o is GameObject go)
                {
                    var modifications = PrefabUtility.GetPropertyModifications(go);
                    if (modifications == null) continue;
                    for (var i = 0; i < modifications.Length; i++)
                    {
                        var prop = modifications[i];
                        prop.value = modify?.Invoke(prop.value);
                        Debug.Log((prop.target?.name ?? "") + ": " + prop.value);
                    }

                    PrefabUtility.SetPropertyModifications(go, modifications);
                    PrefabUtility.SavePrefabAsset(go);
                }
            }

            // AssetDatabase.SaveAssets();
        }

        private static void TraverseChildren(Transform t, Action<Transform> onTraverse)
        {
            onTraverse?.Invoke(t);
            for (var i = 0; i < t.childCount; i++)
            {
                TraverseChildren(t.GetChild(i), onTraverse);
            }
        }

        private static void ChangeTag(string prefix, string newPrefix)
        {
            foreach (var o in Selection.objects)
            {
                var serialized = new SerializedObject(o);
                var prop = serialized.FindProperty("_tags");
                if (prop != null && prop.isArray)
                {
                    for (var i = 0; i < prop.arraySize; i++)
                    {
                        var item = prop.GetArrayElementAtIndex(i);
                        if (!item.stringValue.Contains(prefix)) continue;
                        item.stringValue = item.stringValue.Replace(prefix, newPrefix);
                        Debug.Log(item.stringValue);
                    }
                }

                serialized.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
        }

        private static void ModifyNameOfSelected(Func<string, string> modify)
        {
            for (var i = 0; i < Selection.objects.Length; i++)
            {
                var o = Selection.objects[i];
                var assetPath = AssetDatabase.GetAssetPath(o);
                var pathOnly = Path.GetDirectoryName(assetPath);
                var nameOnly = Path.GetFileNameWithoutExtension(assetPath);
                var extension = Path.GetExtension(assetPath);
                var newName = $"{modify?.Invoke(nameOnly)}{extension}";
                var newAssetPath = Path.Combine(pathOnly ?? "", newName);
                AssetDatabase.MoveAsset(assetPath, newAssetPath);
                Debug.Log(assetPath + "->" + newAssetPath);
            }

            AssetDatabase.Refresh();
        }


        private void ModifyStringFieldOfReferenced(Func<string, string> modify)
        {
            foreach (var asset in Selection.objects)
            {
                var serializedObject = new SerializedObject(asset);
                var props = EnumerateSerializedProperties(serializedObject);
                foreach (var p in props)
                {
                    if (p.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(p.stringValue))
                    {
                        p.stringValue = modify.Invoke(p.stringValue);
                        Debug.Log(p.name + ": " + p.stringValue);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
        }

        private void LogFields()
        {
            foreach (var asset in Selection.objects)
            {
                var props = EnumerateSerializedProperties(asset);
                foreach (var p in props)
                {
                    if (p.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(p.stringValue))
                    {
                        Debug.Log(p.name + ": " + p.stringValue);
                    }
                }
            }
        }

        private void ReplaceReferenceInsidePrefabVariants(Dictionary<string, string[]> map, string propertyName)
        {
            foreach (var asset in Selection.objects)

                if (asset is GameObject go)
                {
                    var added = PrefabUtility.GetAddedGameObjects(go).Select(a => a.instanceGameObject).ToList();
                    added.Add(go);
                    foreach (var aa in added)
                    {
                        var modifications = PrefabUtility.GetPropertyModifications(aa);
                        if (modifications == null || modifications.Length == 0) continue;
                        for (var i = 0; i < modifications.Length; i++)
                        {
                            if (modifications[i].propertyPath.Equals(propertyName))
                            {
                                Debug.Log(modifications[i].propertyPath + " " +
                                          AssetDatabase.GetAssetPath(modifications[i].objectReference));
                                var prop = modifications[i];
                                if (!string.IsNullOrEmpty(modifications[i].value))
                                {
                                    var newAsset = FindNewAsset(AssetDatabase.GUIDToAssetPath(prop.value), map);
                                    prop.value = AssetDatabase.AssetPathToGUID(newAsset);
                                }

                                if (prop.objectReference != null)
                                {
                                    var assets = FindNewAsset(AssetDatabase.GetAssetPath(prop.objectReference), map);
                                    prop.objectReference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assets);
                                }
                            }
                        }

                        PrefabUtility.SetPropertyModifications(aa, modifications);
                    }

                    PrefabUtility.SavePrefabAsset(go);
                }
        }

        private void ReplaceReference(Dictionary<string, string[]> map)
        {
            foreach (var asset in Selection.objects)
            {
                var serializedObject = new SerializedObject(asset);
                // .Where(ContextMenuOpenListener.IsAssetReferenceProperty)
                var props = EnumerateSerializedProperties(serializedObject);
                foreach (var p in props)
                {
                    if (IsAssetReferenceProperty(p))
                    {
                        var refAsset = p.FindPropertyRelative("m_AssetGUID");
                        var path = AssetDatabase.GUIDToAssetPath(refAsset.stringValue);
                        var newAsset = FindNewAsset(path, map);

                        if (newAsset.Equals(path)) return;

                        refAsset.stringValue = AssetDatabase.AssetPathToGUID(newAsset);

                        Debug.Log(p.name + ": " + $"{path}->{newAsset}");
                    }
                    else if (p.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (p.objectReferenceValue != null)
                        {
                            var oldAssetPath = AssetDatabase.GetAssetPath(p.objectReferenceValue);
                            var newAssetPath = FindNewAsset(oldAssetPath, map);
                            if (newAssetPath.Equals(oldAssetPath)) return;
                            Debug.Log(p.name + ": " + $"{oldAssetPath}->{newAssetPath}");
                            p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newAssetPath);
                        }
                        else
                        {
                            Debug.Log(p.name + ": none");
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
        }


        private static string FindNewAsset(string path, Dictionary<string, string[]> map)
        {
            var oldNameWithoutExt = Path.GetFileNameWithoutExtension(path);
            var oldNameExt = Path.GetExtension(path);

            foreach (var d in map)
            {
                var l = d.Key.Length;
                if (oldNameWithoutExt.Length < l) continue;
                var prefixMatched = oldNameWithoutExt[..l].Equals(d.Key);

                if (!prefixMatched) continue;
                foreach (var v in d.Value)
                {
                    var newName = oldNameWithoutExt.Replace(d.Key, v);
                    var foundObject = AssetDatabase.FindAssets(newName).Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(p =>
                            Path.GetFileNameWithoutExtension(p).Equals(newName) &&
                            Path.GetExtension(p).Equals(oldNameExt));
                    if (!string.IsNullOrEmpty(foundObject))
                    {
                        return foundObject;
                    }
                }
            }

            return path;
        }

        private static IEnumerable<SerializedProperty> EnumerateSerializedProperties(SerializedObject serializedObject)
        {
            var iterator = serializedObject.GetIterator();
            while (iterator.Next(true))
            {
                yield return iterator;
            }
        }

        private static IEnumerable<SerializedProperty> EnumerateSerializedProperties(Object @object)
        {
            return EnumerateSerializedProperties(new SerializedObject(@object));
        }

        private static bool IsAssetReferenceProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("m_AssetGUID") != null &&
                   property.FindPropertyRelative("m_SubObjectName") != null;
        }
    }
#endif
}