#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.UnityExtend.Serialization
{
    public static class SerializeUtility
    {
        public static string FormatBackingFieldPropertyName(string name)
        {
            return $"<{name}>k__BackingField";
        }

        public static object GetSiblingProperty(SerializedProperty property, string name)
        {
            var src = GetObjectToWhichPropertyBelong(property);
            return ReflectionUtility.GetDataFromMember(src, name, false);
        }

        public static Type GetSiblingPropertyType(SerializedProperty property, string name)
        {
            var src = GetObjectToWhichPropertyBelong(property);
            var type = src.GetType();
            return ReflectionUtility.GetReturnTypeOfMember(type, name, false);
        }

        public static object GetObjectToWhichPropertyBelong(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = Convert.ToInt32(element[element.IndexOf("[", StringComparison.Ordinal)..]
                        .Replace("[", "").Replace("]", ""));
                    obj = ReflectionUtility.GetValueOfElement(ReflectionUtility.GetDataFromMember(obj, elementName, false) as IEnumerable, index);
                }
                else
                {
                    obj = ReflectionUtility.GetDataFromMember(obj, element, false);
                }
            }

            return obj;
        }        
        public static object GetPropertyValue(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length))
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = Convert.ToInt32(element[element.IndexOf("[", StringComparison.Ordinal)..]
                        .Replace("[", "").Replace("]", ""));
                    obj = ReflectionUtility.GetValueOfElement(ReflectionUtility.GetDataFromMember(obj, elementName, false) as IEnumerable, index);
                }
                else
                {
                    obj = ReflectionUtility.GetDataFromMember(obj, element, false);
                }
            }

            return obj;
        }

        public static void TraverseAllUnityScriptableAssets(Func<UnityEngine.Object, bool> onTraverse)
        {
            TraverseAllPrefabsOrdered(prefab => TraverseRootGameObject(prefab, onTraverse));
            TraverseAllScenes((s) =>
            {
                var modified = false;
                foreach (var go in s.GetRootGameObjects())
                {
                    modified |= TraverseRootGameObject(go, onTraverse);
                }
                return modified;
            });


            TraverseAllScriptableObjects(onTraverse);

            static bool TraverseRootGameObject(GameObject go, Func<UnityEngine.Object, bool> onTraverse)
            {
                if (go == null) return false;
                var modified = false;
                foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (mb == null) continue;
                    modified |= onTraverse?.Invoke(mb) ?? false;
                }
                return modified;
            }
        }
        public static void TraverseAllUnitySerializableObjects(Func<UnityEngine.Object, bool> onTraverse)
        {
            TraverseAllPrefabsOrdered(prefab => TraverseRootGameObject(prefab, onTraverse));
            TraverseAllScenes((s) =>
            {
                var modified = false;
                foreach (var go in s.GetRootGameObjects())
                {
                    modified |= TraverseRootGameObject(go, onTraverse);
                }
                return modified;
            });

            TraverseAllScriptableObjects(onTraverse);

            static bool TraverseRootGameObject(GameObject go, Func<UnityEngine.Object, bool> onTraverse)
            {
                if (go == null) return false;
                var modified = false;
                foreach (var tr in go.GetComponentsInChildren<Transform>())
                {
                    modified |= onTraverse?.Invoke(tr.gameObject) ?? false;
                    foreach (var mb in tr.gameObject.GetComponentsInChildren<Component>())
                    {
                        modified |= onTraverse?.Invoke(mb) ?? false;
                    }
                }
                return modified;
            }
        }

        public static void TraverseAllScriptableObjects(Func<UnityEngine.ScriptableObject, bool> onTraverse)
        {
            var scriptableObjects = GetAllScriptableObjectAssets();
            var modified = false;
            foreach (var so in scriptableObjects)
            {
                modified |= onTraverse?.Invoke(so) ?? false;
            }

            if (modified)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public static void TraverseAllPrefabs(Func<UnityEngine.GameObject, bool> onTraverse)
        {
            var allPrefabs = GetAllPerfabs();
            foreach (var prefab in allPrefabs)
            {
                var modified = onTraverse?.Invoke(prefab) ?? false;
                if (modified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }
        public static void TraverseAllPrefabsOrdered(Func<UnityEngine.GameObject, bool> onTraverse)
        {
            var allPrefabPaths = AssetDatabase.FindAssets($"t: prefab").Select(AssetDatabase.GUIDToAssetPath);
            var traversedPrefabs = new List<string>();
            foreach (var prefabPath in allPrefabPaths)
            {
                if (traversedPrefabs.Contains(prefabPath)) continue;
                TraverseRecursive(prefabPath, traversedPrefabs, onTraverse);
            }
            static void TraverseRecursive(string prefabPath, List<string> traversedPrefabs, Func<UnityEngine.GameObject, bool> onTraverse)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                foreach (var tr in prefab.GetComponentsInChildren<Transform>())
                {
                    if (tr.gameObject == prefab) continue;
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(tr.gameObject))
                    {
                        var prPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tr.gameObject);
                        if (traversedPrefabs.Contains(prPath) || !System.IO.Path.GetExtension(prPath).Equals(".prefab")) continue;
                        TraverseRecursive(prPath, traversedPrefabs, onTraverse);
                    }
                }

                var modified = onTraverse?.Invoke(prefab) ?? false;
                if (modified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }

                traversedPrefabs.Add(prefabPath);
            }
        }

        public static IEnumerable<UnityEngine.ScriptableObject> GetAllScriptableObjectAssets()
        {
            return AssetDatabase.FindAssets($"t: scriptableobject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<UnityEngine.ScriptableObject>();
        }
        public static IEnumerable<UnityEngine.GameObject> GetAllPerfabs()
        {
            return AssetDatabase.FindAssets($"t: prefab").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>);
        }
        public static void TraverseAllScenes(Func<UnityEngine.SceneManagement.Scene, bool> onOpenScene)
        {
            var currentScene = EditorSceneManager.GetActiveScene();//.path;
            var scenePaths = AssetDatabase.FindAssets($"t: scene", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath);
            foreach (var sp in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(sp, OpenSceneMode.Additive);
                var modified = onOpenScene?.Invoke(scene) ?? false;
                if (modified)
                {
                    EditorSceneManager.SaveScene(scene);
                }
                if (scene != currentScene)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
    }
}
#endif
