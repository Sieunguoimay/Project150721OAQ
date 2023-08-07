using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Obsolete]
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
#if UNITY_EDITOR
        if (!Application.isPlaying) return editorAsset as TObject;
#endif
        var runtimeAsset = addressabled ? AddressablesManager.Instance.GetRuntimeAssetByAddress(assetPath, typeof(TObject)) : directAsset;
        if (runtimeAsset is TObject a)
            return a;
        Debug.LogError($"Cannot cast asset to {typeof(TObject)}");
        return null;
    }

    public class AssetTypeAttribute : PropertyAttribute
    {
        public Type type;
        public AssetTypeAttribute(Type t)
        {
            type = t;
        }
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

    private AssetSelector.AssetTypeAttribute _assetTypeAttribute;

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

        var content = _assetTypeAttribute != null ? new GUIContent(label.text + $" ({_assetTypeAttribute.type.Name})") : label;
        var type = _assetTypeAttribute != null ? _assetTypeAttribute.type : typeof(UnityEngine.Object);
        var obj = EditorGUI.ObjectField(position, content, editorAsset.objectReferenceValue, type, true);
        if (obj != editorAsset.objectReferenceValue)
        {
            property.serializedObject.Update();
            editorAsset.objectReferenceValue = obj;
            UpdateAssetPath();
            ValidateAssetPath(property);
            property.serializedObject.ApplyModifiedProperties();
        }

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
        var validType = IsTypeValid(property);
        var addressabled = ValidateAddressable(assetPath.stringValue);
        _valid = addressabled && validType;
    }
    private bool ValidateAddressable(string path)
    {
        return !string.IsNullOrEmpty(path) && AddressablesManager.GetAddressableEntryForAddress(path) != null;
    }

    private bool IsTypeValid(SerializedProperty property)
    {
        _assetTypeAttribute ??= TryGetAttribute<AssetSelector.AssetTypeAttribute>(property);

        if (_assetTypeAttribute == null) return true;

        var type = editorAsset.objectReferenceValue.GetType();

        if (_assetTypeAttribute.type.IsAssignableFrom(type))
        {
            return true;
        }
        return false;
    }
    private TAttribute TryGetAttribute<TAttribute>(SerializedProperty property) where TAttribute : Attribute
    {
        var obj = SerializeUtility.GetObjectToWhichPropertyBelong(property);
        var member = obj.GetType().GetField(property.name, ReflectionUtility.FieldFlags);
        var attr = member.GetCustomAttributes(typeof(AssetSelector.AssetTypeAttribute), false).FirstOrDefault();
        if (attr is TAttribute assetType)
        {
            return assetType;
        }
        return null;
    }
}

#endif

