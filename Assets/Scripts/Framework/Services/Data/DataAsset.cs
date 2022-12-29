using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Misc;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.Services.Data
{
    public class DataAsset : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Use AssetName")]
        private void UseAssetNameAsId()
        {
            Id = name;
        }

        [ContextMenu("Format Name")]
        private void FormatName()
        {
            var newName = StringUtility.SeparateCapitalBySpace(name).Replace(' ', '_').ToLower();
            if (AssetDatabase.IsSubAsset(this))
            {
                name = newName;
                EditorUtility.SetDirty(this);
            }
            else
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), newName);
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(DataAsset), true)]
    [CanEditMultipleObjects]
    public class DataAssetEditor : BaseContextMenuExtendEditor
    {
        
    }
#endif
}