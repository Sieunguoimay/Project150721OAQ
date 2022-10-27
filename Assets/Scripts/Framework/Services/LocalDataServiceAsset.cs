using System.Linq;
using Framework.Entities;
using UnityEngine;

namespace Framework.Services
{
    [CreateAssetMenu]
    public class LocalDataServiceAsset : ScriptableObject, IDataService
    {
        [field: SerializeField] private DataAsset[] Assets { get; set; }

        public TData Load<TData>(string id) where TData : IEntityData
        {
            var found = Assets.FirstOrDefault(a => a is IEntityData d && d.Id.Equals(id));

            if (!found)
            {
                Debug.LogError($"Data not found {id}");
            }

            return (TData) (IEntityData) found;
        }
        
#if UNITY_EDITOR
        [ContextMenu("UpdateAssetList")]
        private void UpdateAssetList()
        {
            var assetGuids = UnityEditor.AssetDatabase.FindAssets($"t: {typeof(DataAsset).FullName}");
            var assets = assetGuids.Select(s => UnityEditor.AssetDatabase.LoadAssetAtPath<DataAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(s)));
            Assets = assets.ToArray();
        }
#endif
    }
}