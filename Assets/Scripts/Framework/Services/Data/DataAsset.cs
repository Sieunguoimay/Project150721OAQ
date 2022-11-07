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
#endif
    }
}