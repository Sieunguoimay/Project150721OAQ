using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph
{
    private class HeaderBar : VisualElement
    {
        private readonly ObjectVisitingPath _visitingPath;
        private readonly AssetDependencyGraph _window;
        private Button _goBackButton;
        public ObjectVisitingPath ObjectTrain => _visitingPath;
        public HeaderBar(AssetDependencyGraph window)
        {
            _window = window;
            _visitingPath = new();
            style.flexDirection = FlexDirection.Row;
            style.justifyContent = Justify.SpaceBetween;
            style.paddingTop = 5;
            Add(CreateLeft());
            Add(CreateRight());
        }
        private VisualElement CreateRight()
        {
            var moreButton = new Button
            {
                text = "...",
                focusable = false
            };
            //moreButton.style.width = 18;
            moreButton.clicked += OnMoreButtonClicked;
            return moreButton;
        }

        private void OnMoreButtonClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new UnityEngine.GUIContent("Ping Script"), false, () =>
            {
                var scriptObject = AssetDatabase.LoadAssetAtPath<Object>(
                    AssetDatabase.FindAssets($"t: Script {nameof(AssetDependencyGraph)}")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(p => p.EndsWith($"{nameof(AssetDependencyGraph)}.cs"))); ; ;
                EditorGUIUtility.PingObject(scriptObject);
            });
            menu.ShowAsContext();
        }

        private VisualElement CreateLeft()
        {
            var left = new VisualElement();
            left.style.flexDirection = FlexDirection.Row;
            left.style.alignItems = Align.FlexStart;
            //left.style.paddingTop = 5;

            var importButton = new Button
            {
                text = "Import",
                focusable = false
            };
            importButton.style.marginRight = 0;
            importButton.clicked += OnImportButtonClicked;

            var dropDownButton = new Button
            {
                text = "...",
                focusable = false
            };
            dropDownButton.style.marginLeft = 0;
            dropDownButton.style.marginRight = 0;
            dropDownButton.clicked += OnDropDownuttonClicked;


            _goBackButton = new Button
            {
                text = "<",
                focusable = false
            };
            _goBackButton.style.marginLeft = 0;
            _goBackButton.style.marginRight = 0;
            _goBackButton.SetEnabled(false);
            _goBackButton.clicked += OnGoBackButtonClicked;

            _visitingPath.OnCurrentChanged += OnObjectTrainChanged;

            left.Add(importButton);
            left.Add(dropDownButton);
            left.Add(_goBackButton);
            left.Add(_visitingPath);
            return left;
        }

        private void OnDropDownuttonClicked()
        {
            var menu = new GenericMenu();
            var sceneAssets = AssetDatabase.FindAssets($"t: Scene").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Object>);

            foreach (var asset in sceneAssets)
            {
                menu.AddItem(new GUIContent(asset.name), false, () =>
                {
                    ChangeRootObject(asset);
                });
            }

            menu.ShowAsContext();
        }

        private void OnGoBackButtonClicked()
        {
            _visitingPath.Back();
        }

        private void OnObjectTrainChanged(ObjectVisitingPath obj)
        {
            _window.Import(_visitingPath.Current);
            _goBackButton.SetEnabled(_visitingPath.Navigatable);
        }
        private void OnImportButtonClicked()
        {
            ChangeRootObject(Selection.activeObject);
        }
        private void ChangeRootObject(Object asset)
        {
            if (asset != null && asset != _visitingPath.Root)
            {
                _visitingPath.ClearStack();
                _visitingPath.Push(asset);
            }
        }
    }
}