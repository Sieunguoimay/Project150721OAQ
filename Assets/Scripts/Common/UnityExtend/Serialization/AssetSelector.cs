using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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

    public class AssetTypeAttribute : PropertyAttribute
    {
        public Type type;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AssetSelector))]
public class AssetSelectorPropertyDrawer : PropertyDrawer
{
    private bool _valid;
    private bool _firstFrame = true;

    private SerializedProperty assetPath;
    private SerializedProperty editorAsset;
    private SerializedProperty directAsset;
    private SerializedProperty addressabled;

    private AssetSelector.AssetTypeAttribute assetTypeAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        assetPath = property.FindPropertyRelative("assetPath");
        editorAsset = property.FindPropertyRelative("editorAsset");
        directAsset = property.FindPropertyRelative("directAsset");
        addressabled = property.FindPropertyRelative("addressabled");

        if (_firstFrame)
        {
            _firstFrame = false;

            ValidateAssetPath(property);
        }

        var color = GUI.color;
        GUI.color = _valid ? color : Color.red;

        property.serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        var content = assetTypeAttribute != null ? new GUIContent(label.text + $" ({assetTypeAttribute.type.Name})") : label;
        EditorGUI.PropertyField(position, editorAsset, content, false);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateAssetPath();
            ValidateAssetPath(property);
        }

        property.serializedObject.ApplyModifiedProperties();

        GUI.color = color;
    }

    private void UpdateAssetPath()
    {
        if (editorAsset.objectReferenceValue != null)
        {
            var found = AddressablesManager.GetAddressableEntryForAsset(editorAsset.objectReferenceValue);
            if (found != null)
            {
                assetPath.stringValue = found?.address;
                addressabled.boolValue = true;
                directAsset.objectReferenceValue = null;
            }
            else
            {
                assetPath.stringValue = "";
                addressabled.boolValue = false;
                directAsset.objectReferenceValue = editorAsset.objectReferenceValue;
            }
        }
    }
    private void ValidateAssetPath(SerializedProperty property)
    {
        assetTypeAttribute ??= TryGetAttribute(property);

        var path = assetPath.stringValue;
        if (!string.IsNullOrEmpty(path))
        {
            _valid = ValidateAddressable(path) && IsValid(property);
        }
    }
    private bool ValidateAddressable(string path)
    {
        return !string.IsNullOrEmpty(path) && AddressablesManager.GetAddressableEntryForAddress(path) != null;
    }

    private bool IsValid(SerializedProperty property)
    {
        if (assetTypeAttribute == null) return true;

        var type = editorAsset.objectReferenceValue.GetType();

        if (assetTypeAttribute.type.IsAssignableFrom(type))
        {
            return true;
        }
        return false;
    }
    private AssetSelector.AssetTypeAttribute TryGetAttribute(SerializedProperty property)
    {
        var editorAsset = property.FindPropertyRelative("editorAsset");
        if (editorAsset.objectReferenceValue == null) return null;
        var obj = SerializeUtility.GetObjectToWhichPropertyBelong(property);
        var member = obj.GetType().GetField(property.name, ReflectionUtility.FieldFlags);
        var attr = member.GetCustomAttribute(typeof(AssetSelector.AssetTypeAttribute), false);
        if (attr is AssetSelector.AssetTypeAttribute assetType)
        {
            return assetType;
        }
        return null;
    }
}

#endif

