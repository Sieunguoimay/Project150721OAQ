#if UNITY_EDITOR
using Common.UnityExtend.UIElements.GraphView;
using Common.UnityExtend.UIElements.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph
{
    private class FoldableDependencyNode : DependencyNode
    {
        private FoldoutButton _foldOutButton;
        private bool _foldout = false;
        private readonly List<DependencyNode> _subNodes = new();
        private readonly List<DependencyEdge> _toggledEdges = new();
        private readonly GraphView _graph;
        public IEnumerable<DependencyNode> SubNodes => _subNodes;

        public FoldableDependencyNode(GraphView graph, string path) : base(path)
        {
            _graph = graph;
            OnMove += OnNodeMove;
        }

        public void Init()
        {
            LoadChildren();
            if (_subNodes.Count > 0)
            {
                _foldOutButton = new FoldoutButton();
                _foldOutButton.style.position = Position.Absolute;
                _foldOutButton.style.width = 10;
                _foldOutButton.style.height = 10;
                _foldOutButton.style.left = -16;
                _foldOutButton.Clicked += OnFoldoutButtonClicked;
                Add(_foldOutButton);
            }
            for (int i = 0; i < _subNodes.Count; i++)
            {
                var sn = _subNodes[i];

                var x = style.left.value.value + 10;
                var y = style.top.value.value + (i + 1) * 40;

                sn.DefaultPosition = new Vector2(x, y);
            }

        }

        private void OnNodeMove(NodeView view)
        {
            UpdateSubNodesPosition();
        }

        private void UpdateSubNodesPosition()
        {
            for (int i = 0; i < _subNodes.Count; i++)
            {
                var sn = _subNodes[i];

                var x = style.left.value.value + 10;
                var y = style.top.value.value + (i + 1) * 40;

                sn.style.left = x;
                sn.style.top = y;
                sn.InvokeMoveEvent();
            }
        }

        private void OnFoldoutButtonClicked(FoldoutButton evt)
        {
            _foldout = !_foldout;
            _foldOutButton.SetRotateDown(_foldout);
            UpdateFoldoutChildren();
        }
        private void UpdateFoldoutChildren()
        {
            if (_foldout)
            {
                AddVisualElements();
            }
            else
            {
                RemoveVisualElements();
            }
        }

        private void RemoveVisualElements()
        {
            foreach (var sn in _subNodes)
            {
                _graph.RemoveNode(sn);
            }
            foreach (var e in _toggledEdges)
            {
                _graph.RemoveEdge(e);
            }

            foreach (var e in ConnectedEdges)
            {
                if (_toggledEdges.Contains(e)) continue;
                _graph.AddEdge(e);
            }
        }

        private void AddVisualElements()
        {
            foreach (var sn in _subNodes)
            {
                _graph.AddNode(sn);
            }

            UpdateSubNodesPosition();

            foreach (var e in _toggledEdges)
            {
                _graph.AddEdge(e);
            }

            foreach (var e in ConnectedEdges)
            {
                if (_toggledEdges.Contains(e)) continue;
                _graph.RemoveEdge(e);
            }
        }

        private void LoadChildren()
        {
            var objectTexts = LoadAllObjectTexts(Path);
            var objectTextDict = objectTexts.ToDictionary(ExtractLocalID, t => t);
            var subObjects = LoadSubObjects(objectTextDict);

            if (!subObjects.Any()) return;

            var dependencyNodes = QueryConnectedNodes(true)
                .Select(n => n as DependencyNode).ToDictionary(n => n.Path, n => n);
            var subDependenciesDict = objectTexts.ToDictionary(
                t => ExtractLocalID(t),
                t => DependencyGraphCreator.ParseReferencesAlongWithFieldName(t)
                    .Where(d => !d.Item1.Equals("m_CorrespondingSourceObject")));

            var isPrefab = Target is GameObject;

            foreach (var subObject in subObjects)
            {
                CreateSubNodesAndConnect(dependencyNodes, subDependenciesDict, isPrefab, subObject);
            }
        }

        private void CreateSubNodesAndConnect(Dictionary<string, DependencyNode> dependencyNodes, Dictionary<long, IEnumerable<(string, string)>> subDependenciesDict, bool isPrefab, (long, Object) subObject)
        {
            if (subDependenciesDict.TryGetValue(subObject.Item1, out var ds))
            {
                var connectNodes = FindConnectNodes(dependencyNodes, ds);
                if (connectNodes.Length > 0)
                {
                    if (subObject.Item2 != Target)
                    {
                        CreateSubNodeAndEdges(isPrefab, subObject, ds, connectNodes);
                    }
                    else
                    {
                        CreateEdgesToMainNode(connectNodes);
                    }
                }
            }
        }

        private static (string fieldName, DependencyNode node)[] FindConnectNodes(Dictionary<string, DependencyNode> dependencyNodes, IEnumerable<(string, string)> ds)
        {
            return ds
                .Select(d => dependencyNodes.TryGetValue(AssetDatabase.GUIDToAssetPath(d.Item2), out var node) ? (fieldName: d.Item1, node) : (fieldName: d.Item1, null))
                .Where(n => n.node != null).GroupBy(n => n.node).Select(g => g.First()).ToArray();
        }

        private void CreateEdgesToMainNode((string fieldName, DependencyNode node)[] connectNodes)
        {
            foreach (var (_, node) in connectNodes)
            {
                var dependencyEdge = new DependencyEdge();
                dependencyEdge.Connect(this, node);
                _toggledEdges.Add(dependencyEdge);
            }
        }

        private void CreateSubNodeAndEdges(bool isPrefab, (long, Object) subObject, IEnumerable<(string, string)> ds, (string fieldName, DependencyNode node)[] connectNodes)
        {
            var displayName = isPrefab ? $"{subObject.Item2.name} ({subObject.Item2.GetType().Name})" : subObject.Item2.name;
            string tooltip = GetTooltipForSubObject(isPrefab, subObject, ds);

            var subNode = new DependencyNode(subObject.Item2, displayName)
            {
                tooltip = tooltip
            };
            _subNodes.Add(subNode);
            foreach (var (_, node) in connectNodes)
            {
                var dependencyEdge = new DependencyEdge();
                dependencyEdge.Connect(subNode, node);
                _toggledEdges.Add(dependencyEdge);
            }
        }

        private string GetTooltipForSubObject(bool isPrefab, (long, Object) subObject, IEnumerable<(string, string)> ds)
        {
            var path = subObject.Item2.name;
            if (subObject.Item2 is Component c && TraversePathForLocalGameObject((Target as GameObject).transform, c.transform, out var p))
            {
                path = p;
            }
            else if (subObject.Item2 is GameObject go && TraversePathForPrefabInstance((Target as GameObject).transform, go.transform, out var p2))
            {
                path = p2;
            }
            var fields = string.Join(", ", ds.Select(cn => cn.Item1));
            var tooltip = isPrefab ? $"- path: {path}\n- fields: {fields}" : $"- fields: {fields}";
            return tooltip;
        }

        private static bool TraversePathForLocalGameObject(Transform root, Transform target, out string path)
        {
            path = root.name;
            if (root == target)
            {
                return true;
            }
            foreach (Transform c in root.transform)
            {
                if (TraversePathForLocalGameObject(c, target, out var p))
                {
                    path = $"{path}/{p}";
                    return true;
                }
            }
            return false;
        }
        private static bool TraversePathForPrefabInstance(Transform root, Transform target, out string path)
        {
            path = root.name;
            var prefabRoot = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root.gameObject);
            var prefabRoot2 = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target.gameObject);
            if (prefabRoot == prefabRoot2)
            {
                return true;
            }
            foreach (Transform c in root.transform)
            {
                if (TraversePathForPrefabInstance(c, target, out var p))
                {
                    path = $"{path}/{p}";
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<(long, Object)> LoadSubObjects(Dictionary<long, string> objectTextDict)
        {
            bool isSceneAsset = Path.EndsWith(".unity");
            if (isSceneAsset)
            {
                return new (long, Object)[0];
            }
            var localIds = objectTextDict.Keys.ToArray();
            if (AssetDatabase.LoadMainAssetAtPath(Path) is GameObject go)
            {
                return LoadSubObjectsOfPrefab(objectTextDict, localIds);
            }
            else
            {
                return LoadSubObjectsOfAsset(localIds);
            }
        }

        private IEnumerable<(long, Object)> LoadSubObjectsOfAsset(long[] localIds)
        {
            return AssetDatabase.LoadAllAssetsAtPath(Path)
                .Select(a => (localId: AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out var _, out long localid) ? localid : 0, obj: a))
                .Where(a => localIds.Contains(a.localId))
                .OrderBy(d => System.Array.IndexOf(localIds, d.localId));
        }

        private IEnumerable<(long, Object)> LoadSubObjectsOfPrefab(Dictionary<long, string> objectTextDict, long[] localIds)
        {
            return AssetDatabase.LoadAllAssetsAtPath(Path)
                .Select(a => (localId: AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out var _, out long localid) ? localid : 0, obj: a))
                .Where(a => localIds.Contains(a.localId))
                .Select(a => (a.obj is not Component && a.obj is not GameObject)
                    ? (a.localId, obj: GetPrefabInstanceObject(objectTextDict, a.localId)) : a)
                .OrderBy(d => System.Array.IndexOf(localIds, d.localId));
        }

        private static Object GetPrefabInstanceObject(Dictionary<long, string> objectTextDict, long localId)
        {
            if (objectTextDict.TryGetValue(localId, out var objectText))
            {
                var prefabInstance = GetPrefabInstanceObject(objectText);
                if (prefabInstance != null)
                {
                    return prefabInstance;
                }
            }

            var prefabInstancePlaceholder = ScriptableObject.CreateInstance<ScriptableObject>();
            prefabInstancePlaceholder.name = "PrefabInstancePlaceholder";

            return prefabInstancePlaceholder;
        }
        private static Object GetPrefabInstanceObject(string objectText)
        {
            var found = DependencyGraphCreator.ParseReferencesAlongWithFieldName(objectText).FirstOrDefault(p => p.Item1.Equals("m_SourcePrefab"));
            if (found.Item1 != null && found.Item2 != null)
            {
                var targetObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(found.Item2));

                var namePattern = @"propertyPath:\s*m_Name\s*\n\s*value:\s*(.+)";
                var match = Regex.Match(objectText, namePattern);

                if (match.Success)
                {
                    string extractedValue = match.Groups[1].Value.Trim();
                    targetObject.name = extractedValue;
                }
                return targetObject;
            }
            return null;
        }

        private static long ExtractLocalID(string yamlString)
        {
            var pattern = @"(?<=&)[+-]?\d+";
            var match = Regex.Match(yamlString, pattern);

            if (match.Success)
            {
                var localIDString = match.Groups[0].Value;
                var localID = long.Parse(localIDString);

                return localID;
            }

            return -1;
        }

        private static IEnumerable<string> LoadAllObjectTexts(string path)
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, path["Assets/".Length..]);

            if (File.Exists(fullPath))
            {
                var assetText = File.ReadAllText(fullPath);
                return YamlParse(assetText);
            }
            return new string[0];
        }
        public static IEnumerable<string> YamlParse(string yamlText)
        {
            int startIndex = yamlText.IndexOf("--- !u!");

            if (startIndex != -1)
            {
                string separator = "--- !u!";
                string[] unityObjects = yamlText[startIndex..].Split(new string[] { separator }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (string unityObjectYAML in unityObjects)
                {
                    yield return unityObjectYAML;
                }
            }
            else
            {
                Debug.LogError("Invalid YAML format: Could not find Unity objects.");
            }
        }

        public class FoldoutButton : VisualElement
        {
            private bool _down;
            public event System.Action<FoldoutButton> Clicked;
            public FoldoutButton()
            {
                generateVisualContent += OnRepaint;
                RegisterCallback<MouseDownEvent>(OnMouseDown);
                RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            private void OnMouseUp(MouseUpEvent evt)
            {
                if (evt.button == 0)
                {
                    if (_down)
                    {
                        _down = false;
                        Clicked?.Invoke(this);
                    }
                    evt.StopPropagation();
                }
            }

            private void OnMouseDown(MouseDownEvent evt)
            {
                if (evt.pressedButtons == 1)
                {
                    _down = true;
                    evt.StopPropagation();
                }
            }

            public void SetRotateDown(bool rotateDown)
            {
                var angle = rotateDown ? (90f) : 0f;
                style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree))); ;
            }

            private void OnRepaint(MeshGenerationContext context)
            {
                Painter2DUtility.FillTriangle(context,
                    new Vector2(0, 0),
                    new Vector2(contentRect.width, contentRect.height / 2f),
                    new Vector2(0, contentRect.height),
                    Color.gray);
            }
        }
    }
}
#endif