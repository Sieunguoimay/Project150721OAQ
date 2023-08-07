#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sieunguoimay.Serialization.Tools
{
    [EditorWindowTitle(title = "AssetReferenceReplaceWindow")]
    public class AssetReferenceReplaceWindow : EditorWindow
    {
        [MenuItem("Tools/Snm/AssetReferenceReplaceWindow")]
        public static void Open()
        {
            var window = GetWindow<AssetReferenceReplaceWindow>(false, "AssetReferenceReplaceWindow", true);
            window.Show();
        }

        private string _referencesFolder;
        private PreviewItem[] _previewItems;
        private Object[] _selectedObjects;
        private Vector2 _scrollPos;
        private Vector2 _scrollPos2;
        private bool _showAllPreview;

        private string _guidFinder;
        private static Type[] _allTypes;
        private static IEnumerable<Type> AllTypes => _allTypes ??= typeof(AssetReferenceReplaceWindow).Assembly.GetTypes();
        private void OnGUI()
        {
            DrawGUIDFinder();
            _referencesFolder = DrawAssetSelector(_referencesFolder, "New references folder");
            DrawReferenceReplace();
            DrawLoadedItems();
        }
        private void DrawGUIDFinder()
        {
            EditorGUILayout.BeginHorizontal();
            _guidFinder = GUILayout.TextField(_guidFinder);
            if (GUILayout.Button("Find"))
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(_guidFinder));
            }
            EditorGUILayout.EndHorizontal();
        }

        public static string DrawAssetSelector(string folder, string label)
        {
            EditorGUILayout.BeginHorizontal();
            folder = EditorGUILayout.TextField(new GUIContent($"{label}"), folder);
            if (GUILayout.Button("Use selected", GUILayout.Width(120)))
            {
                var p = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (AssetDatabase.IsValidFolder(p))
                {
                    folder = p;
                }
                else
                {
                    folder = Path.GetDirectoryName(p);
                }
            }
            EditorGUILayout.EndHorizontal();
            return folder;
        }

        private void DrawReferenceReplace()
        {
            EditorGUILayout.BeginHorizontal();
            var preview = GUILayout.Button("Load assets from selection");
            var replace = GUILayout.Button("Replace");
            var clear = GUILayout.Button("Clear");
            _showAllPreview = GUILayout.Toggle(_showAllPreview, GUIContent.none, GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();

            if (preview) LoadPreviewV2();
            if (replace) Replace2();
            if (clear) Clear();
        }

        private void DrawLoadedItems()
        {
            EditorGUILayout.BeginVertical();
            _scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2, GUILayout.MaxHeight(250));
            DrawSelectedObjects();
            EditorGUILayout.EndScrollView();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawPreviewItems();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        private void LoadPreviewV2()
        {
            var paths = LoadObjectsFromSelection().Select(AssetDatabase.GetAssetPath).ToArray();
            var previewItems = new List<PreviewItem>();
            TraverseAssets2(paths, (guid) =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var oldObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                var newObject = FindCorrespondingAssetInFolder(oldObject, _referencesFolder);
                previewItems.Add(new PreviewItem()
                {
                    propertyPath = "",
                    targetObject = null,
                    propType = null,
                    oldObject = oldObject,
                    newObject = newObject,
                    isAValidReplace = newObject != null && newObject != oldObject
                });
                return "";
            }, false);
            _previewItems = previewItems.ToArray();
        }
        private void Replace2()
        {
            var count = 0;
            var paths = LoadObjectsFromSelection().Select(AssetDatabase.GetAssetPath).ToArray();
            var previewItems = new List<PreviewItem>();
            TraverseAssets2(paths, (guid) =>
            {
                count++;
                return FindCorrespondingGuidInFolder(guid, _referencesFolder);
            }, true);
            Debug.Log($"Replaced {count} assets");
        }

        private void Clear()
        {
            _selectedObjects = null;
            _previewItems = null;
        }
        private void DrawSelectedObjects()
        {
            if (_selectedObjects == null) return;
            foreach (var o in _selectedObjects)
            {
                EditorGUILayout.ObjectField(o, typeof(Object), false);
            }
        }

        private void DrawPreviewItems()
        {
            if (_previewItems == null) return;
            var index = 0;
            foreach (var item in _previewItems)
            {
                if (!_showAllPreview && !item.isAValidReplace) continue;

                var label = item.ShouldReplace ? "->" : "X";
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent($"{index++}: {item.propertyPath} {item.targetObject?.name} {item.targetObject?.GetType()}"));
                EditorGUILayout.ObjectField(item.oldObject, typeof(Object), true);
                if (item.isAValidReplace)
                {
                    if (GUILayout.Button(new GUIContent(label), GUILayout.Width(30)))
                    {
                        item.ignore = !item.ignore;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent(label), GUILayout.Width(30));
                }
                EditorGUILayout.ObjectField(item.newObject, typeof(Object), true);
                EditorGUILayout.EndHorizontal();
            }
        }
        private IEnumerable<Object> LoadObjectsFromSelection()
        {
            foreach (var o in Selection.objects)
            {
                var p = AssetDatabase.GetAssetPath(o);
                if (AssetDatabase.IsValidFolder(p))
                {
                    foreach (var fo in AssetDatabase.FindAssets("", new[] { p }).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Object>))
                    {
                        var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(fo));
                        foreach (var ffo in assets)
                        {
                            yield return ffo;
                        }
                    }
                }
                else
                {
                    var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(o));
                    foreach (var fo in assets)
                    {
                        yield return fo;
                    }
                }
            }
        }

        private static void TraverseAssets2(IEnumerable<string> assetPaths, Func<string, string> onGuid, bool modifying)
        {
            if (assetPaths == null) return;
            var paths = assetPaths.ToList();
            foreach (var assetPath in assetPaths)
            {
                paths.Add(assetPath + ".meta");
            }
            foreach (var assetPath in paths)
            {
                var p = assetPath.Substring("Assets/".Length);
                var r = Application.dataPath;
                string fullPath = Path.Combine(r, p);
                //Debug.Log($"{r} {p} {assetPath} {fullPath}");
                if (File.Exists(fullPath))
                {
                    string assetText = File.ReadAllText(fullPath);
                    var pattern = @"\{fileID:\s(-?\d+),\s+guid:\s+([\w\d]+)(?:,\s+type:\s+(\d+))?\}";
                    string guidPattern = @"guid:\s+([\w\d]+)";

                    if (modifying)
                    {
                        var newAssetText = ModifyMatches(assetText, pattern, (str) =>
                        {
                            return ModifyMatches(str, guidPattern, p =>
                            {
                                var oldGuid = ParseGUID(p);
                                if (!string.IsNullOrEmpty(oldGuid))
                                {
                                    var newGuid = onGuid?.Invoke(oldGuid);
                                    //Debug.Log($"Result: {oldGuid} -> " + newGuid);
                                    return $"guid: {newGuid}";
                                }
                                return p;
                            });
                        });
                        File.WriteAllText(fullPath, newAssetText);

                        //AssetDatabase.Refresh();
                        //GameObject modifiedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        //PrefabUtility.SavePrefabAsset(modifiedPrefab);
                    }
                    else
                    {
                        var matches = Regex.Matches(assetText, pattern);
                        foreach (Match match in matches)
                        {
                            var guid = ParseGUID(match.Value);
                            if (!string.IsNullOrEmpty(guid))
                            {
                                var newGuid = onGuid?.Invoke(guid);
                                //Debug.Log($"{guid} {AssetDatabase.GUIDToAssetPath(guid)}");
                            }
                        }
                    }
                }
            }
            static string ParseGUID(string referenceString)
            {
                string guidPattern = @"guid:\s+([\w\d]+)";
                Match match = Regex.Match(referenceString, guidPattern);

                if (match.Success && match.Groups.Count >= 2)
                {
                    Group guidGroup = match.Groups[1];
                    return guidGroup.Value;
                }

                return null;
            }
            static string ModifyMatches(string input, string pattern, Func<string, string> modificationFunc)
            {
                return Regex.Replace(input, pattern, match =>
                {
                    var newValue = modificationFunc(match.Value);
                    //Debug.Log($"{match.Value}->{newValue}");
                    return newValue;
                });
            }
            //static string AppendSuffixToFileName(string filePath, string suffix)
            //{
            //    string directory = Path.GetDirectoryName(filePath);
            //    string fileName = Path.GetFileNameWithoutExtension(filePath);
            //    string extension = Path.GetExtension(filePath);
            //    string newFileName = fileName + suffix + extension;
            //    string newFilePath = Path.Combine(directory, newFileName);

            //    return newFilePath;
            //}
        }

        public static string FindCorrespondingGuidInFolder(string guid, string folder)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path);
            var fileName = Path.GetFileName(path);
            var foundPaths = AssetDatabase.FindAssets($"{name}", new[] { folder })
                .Select(f => AssetDatabase.GUIDToAssetPath(f))
                .Where(f => Path.GetFileName(f).Equals(fileName));
            if (!foundPaths.Any()) return guid;
            var foundPath = GetMostMatchingString(foundPaths, path);
            if (foundPath == path)
            {
                return guid;
            }
            //Debug.Log($"FindCorrespondingGuidInFolder {foundPath} {AssetDatabase.AssetPathToGUID(foundPath)}");
            return AssetDatabase.AssetPathToGUID(foundPath);
        }
        public static Object FindCorrespondingAssetInFolder(Object asset, string folder)
        {
            if (string.IsNullOrEmpty(folder) || asset == null) return null;
            var path = AssetDatabase.GetAssetPath(asset);
            var fileName = Path.GetFileName(path);
            var foundPaths = AssetDatabase.FindAssets($"{asset.name}", new[] { folder })
                .Select(f => AssetDatabase.GUIDToAssetPath(f))
                .Where(f => Path.GetFileName(f).Equals(fileName));

            if (!foundPaths.Any()) return null;

            var foundPath = GetMostMatchingString(foundPaths, path);
            if (foundPath == path)
            {
                return asset;
            }

            if (AssetDatabase.IsSubAsset(asset))
            {
                var t = asset.GetType();
                var found = AssetDatabase.LoadAllAssetsAtPath(foundPath).FirstOrDefault(a => !AssetDatabase.IsMainAsset(a) && a.name.Equals(asset.name));
                if (found != null)
                {

                    return found;
                }
            }
            else
            {
                var found = AssetDatabase.LoadAssetAtPath<Object>(foundPath);
                //if (found is GameObject go)
                //{
                //    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long localId))
                //    {
                //        var found2 = go.GetComponentsInChildren<Component>(true).FirstOrDefault(o =>
                //            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out var guid2, out long localId2) && localId == localId2);
                //        return found2;
                //    }
                //}
                return found;
            }
            return null;
        }
        private static string GetMostMatchingString(IEnumerable<string> strings, string input)
        {
            var results = strings.Select(s => new
            {
                String = s,
                Score = CalculateScore(s, input)
            });

            return results.OrderByDescending(r => r.Score).First().String;

            static int CalculateScore(string source, string target)
            {
                string[] sourceSegments = source.Split('/');
                string[] targetSegments = target.Split('/');

                int score = 0;

                int sourceIndex = 0;
                int targetIndex = 0;

                while (sourceIndex < sourceSegments.Length && targetIndex < targetSegments.Length)
                {
                    if (sourceSegments[sourceIndex] == targetSegments[targetIndex])
                    {
                        score++;
                        targetIndex++;
                    }

                    sourceIndex++;
                }

                return score;
            }
        }
        [Serializable]
        private class PreviewItem
        {
            public string propertyPath;
            public Object targetObject;
            public Type propType;
            public Object oldObject;
            public Object newObject;
            public bool isAValidReplace;
            public bool ignore;

            public bool ShouldReplace => isAValidReplace && !ignore;
        }
    }
}
#endif
