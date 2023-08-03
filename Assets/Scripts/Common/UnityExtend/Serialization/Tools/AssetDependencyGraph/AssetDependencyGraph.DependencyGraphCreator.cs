using Common.UnityExtend.UIElements.GraphView;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph
{
    private static class DependencyGraphCreator
    {
        public static NodeView CreateGraph(Object rootObject, out List<NodeView> nodes, out List<EdgeView> edges)
        {
            var rootPath = AssetDatabase.GetAssetPath(rootObject);

            edges = new List<EdgeView>();

            var nodeDict = new Dictionary<string, NodeArangementItem>();
            var rootNode = TraverseToCreateGraph(nodeDict, edges, rootPath, new Vector2(0, 0), 0);
            rootNode.depth = 0;
            rootNode.node.style.left = 0;
            rootNode.node.style.top = 0;
            nodes = nodeDict.Values.Select(a => a.node).ToList();

            ArrangeNodes(nodeDict.Values.ToArray());

            return rootNode.node;
        }
        private static void ArrangeNodes(IReadOnlyList<NodeArangementItem> arangementItems)
        {
            var columns = arangementItems.OrderBy(i => i.depth).GroupBy(i => i.depth);

            var x = 0f;
            foreach (var column in columns)
            {
                var columnItems = column.ToArray();
                var height = (columnItems.Length - 1) * 60f;
                var parents = columnItems.Where(i => i.parent != null).Select(i => i.parent).Distinct().ToArray();
                var parent = parents.FirstOrDefault();
                var sameParent = parents.Count() <= 1;
                var y = (sameParent && parent != null ? (parent.node.style.top.value.value + parent.node.style.height.value.value / 2f) : 0f) - height / 2f;
                //Debug.Log($"=> {string.Join(",", parents.SelectMany(p => p.node.hierarchy.Children().Select(c => (c as ObjectField).value.name)))}");
                foreach (var g in columnItems)
                {
                    g.node.style.left = x;
                    g.node.style.top = y;
                    g.node.DefaultPosition = new Vector2(x, y);
                    y += 60f;
                }

                x += 200f;
            }
        }
        private class NodeArangementItem
        {
            public NodeView node;
            public NodeArangementItem parent;
            public List<NodeArangementItem> children;
            public int depth;
        }

        private static NodeArangementItem TraverseToCreateGraph(Dictionary<string, NodeArangementItem> nodeDict, List<EdgeView> edgeList, string path, Vector2 pos, int depth)
        {
            var parentNode = new NodeArangementItem { node = CreateNode(path), depth = int.MaxValue, children = new() };
            nodeDict.Add(path, parentNode);
            var paths = GetReferences(path);
            foreach (var p in paths)
            {
                if (p.Equals(path)) continue;
                var foundInDict = nodeDict.TryGetValue(p, out var dNode);
                if (!foundInDict)
                {
                    dNode = TraverseToCreateGraph(nodeDict, edgeList, p, new Vector2(pos.x + 200f, pos.y), depth + 1);
                }
                if (dNode.depth > depth + 1)
                {
                    parentNode.children.Add(dNode);
                    dNode.parent = parentNode;
                    dNode.depth = depth + 1;

                    if (foundInDict)
                    {
                        UpdateNodeDepthRecurr(dNode);
                    }
                }
                if (dNode != parentNode)
                {
                    var edge = new EdgeView();
                    edge.Connect(parentNode.node, dNode.node);
                    edgeList.Add(edge);
                }
            }

            return parentNode;
        }
        private static void UpdateNodeDepthRecurr(NodeArangementItem item)
        {
            foreach (var c in item.children)
            {
                c.depth = item.depth + 1;
                UpdateNodeDepthRecurr(c);
            }
        }
        private static string[] GetReferences(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));

            if (File.Exists(fullPath))
            {
                string assetText = File.ReadAllText(fullPath);
                return ParseReferences(assetText).Distinct()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => !string.IsNullOrEmpty(p) && !AssetDatabase.IsValidFolder(p) && p.Contains('.')).ToArray();
            }
            return new string[0];
        }
        private static IEnumerable<string> ParseReferences(string text)
        {
            var referencePattern = @"\{fileID:\s(-?\d+),\s+guid:\s+([\w\d]+)(?:,\s+type:\s+(\d+))?\}";
            string guidPattern = @"guid:\s+([\w\d]+)";

            var matches = Regex.Matches(text, referencePattern);

            foreach (Match match in matches)
            {
                var m = Regex.Match(match.Value, guidPattern);

                if (m.Success && m.Groups.Count >= 2)
                {
                    var guidGroup = m.Groups[1];
                    yield return guidGroup.Value;
                }
            }
        }
        private static NodeView CreateNode(string path)
        {
            var node = new NodeView();
            node.style.flexDirection = FlexDirection.Column;
            node.Add(new DependencyNodeVisual(path));
            return node;
        }
    }
}