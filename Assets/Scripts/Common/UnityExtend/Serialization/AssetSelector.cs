using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class AssetSelector
{
    [SerializeField] private string assetPath;
    [SerializeField] private UnityEngine.Object directAsset;
    [SerializeField] private bool addressabled;

#if UNITY_EDITOR
    [SerializeField] private UnityEngine.Object editorAsset;

    public void UpdateSerialize()
    {
        var found = AddressablesManager.GetAddressableEntryForAsset(editorAsset);
        if (found != null)
        {
            addressabled = true;
            assetPath = found.address;
            directAsset = null;
        }
        else
        {
            addressabled = false;
            assetPath = "";
            directAsset = editorAsset;
        }
    }
#endif
    public TObject GetAsset<TObject>() where TObject : UnityEngine.Object
    {
        if (!Application.isPlaying) return editorAsset as TObject;

        var runtimeAsset = addressabled ? AddressablesManager.Instance.GetRuntimeAssetByAddress(assetPath) : directAsset;
        if (runtimeAsset is TObject a)
            return a;
        Debug.LogError($"Cannot cast asset to {typeof(TObject)}");
        return null;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(AssetSelector))]
public class AssetSelectorPropertyDrawer : PropertyDrawer
{
    private bool _valid;
    private bool _firstFrame = true;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var assetPath = property.FindPropertyRelative("assetPath");
        var editorAsset = property.FindPropertyRelative("editorAsset");
        if (_firstFrame)
        {
            _firstFrame = false;
            if (!string.IsNullOrEmpty(assetPath.stringValue))
            {
                ValidateAssetPath(assetPath.stringValue);
            }
        }

        var color = GUI.color;
        GUI.color = _valid ? color : Color.red;

        property.serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, editorAsset, label, false);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateAssetPath(assetPath, editorAsset);
        }

        property.serializedObject.ApplyModifiedProperties();

        GUI.color = color;
    }

    private void UpdateAssetPath(SerializedProperty assetPath, SerializedProperty editorAsset)
    {
        if (editorAsset.objectReferenceValue != null && editorAsset.objectReferenceValue)
        {
            var path = AddressablesManager.GetAddressableEntryForAsset(editorAsset.objectReferenceValue)?.address;
            assetPath.stringValue = path;
        }
    }
    private void ValidateAssetPath(string path)
    {
        _valid = ValidateAddressable(path);
    }
    private bool ValidateAddressable(string path)
    {
        return !string.IsNullOrEmpty(path) && AddressablesManager.GetAddressableEntryForAddress(path) != null;
    }
}
#endif

