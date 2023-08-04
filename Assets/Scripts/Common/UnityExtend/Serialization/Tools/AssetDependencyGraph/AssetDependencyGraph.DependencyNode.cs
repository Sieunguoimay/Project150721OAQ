#if UNITY_EDITOR
using Common.UnityExtend.UIElements.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph
{
    private class DependencyNode : NodeView
    {
        private readonly Object _target;
        private readonly Texture _icon;
        private Label _label;
        public Object Target => _target;
        public string Path { get; private set; }

        public Vector2 defaultPosition;

        public DependencyNode(string path)
        {
            Path = path;
            _target = AssetDatabase.LoadAssetAtPath<Object>(path);
            _icon = AssetDatabase.GetCachedIcon(path);

            Setup();

            _label.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _label.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _label.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        public DependencyNode(Object target)
        {
            Path = AssetDatabase.GetAssetPath(target);
            _target = target;
            _icon = EditorGUIUtility.ObjectContent(target, null).image;

            Setup();

            _label.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _label.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _label.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _label.style.backgroundColor = new Color(.1f, .1f, .1f, .5f);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _label.style.backgroundColor = new Color(.05f, .05f, .05f, .8f);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.pressedButtons == 1)
            {
                EditorGUIUtility.PingObject(_target);
                evt.StopPropagation();
            }
        }

        private void Setup()
        {
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            var image = new Image
            {
                image = _icon
            };
            image.style.width = 16;
            image.style.height = 16;
            Add(image);

            var label = new Label
            {
                text = _target != null ? _target.name : Path
            };
            label.style.position = Position.Absolute;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.left = 25;
            label.style.backgroundColor = new Color(.1f, .1f, .1f, .5f);
            Add(label);
            _label = label;
        }
    }
}
#endif