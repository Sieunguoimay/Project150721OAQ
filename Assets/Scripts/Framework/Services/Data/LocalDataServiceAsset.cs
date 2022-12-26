using System.Linq;
using Framework.Entities;
using UnityEngine;

namespace Framework.Services.Data
{
    [CreateAssetMenu]
    public class LocalDataServiceAsset : ScriptableObject, IDataService
    {
        [field: SerializeField] public DataAsset[] Assets { get; private set; }

        public object Load(string id)
        {
            var found = Assets.FirstOrDefault(a => a.Id.Equals(id));

            if (!found)
            {
                Debug.LogError($"Data not found {id}");
            }

            return found;
        }

        public TData Load<TData>(string id) where TData : IEntityData
        {
            var found = Assets.FirstOrDefault(a => a is TData && a.Id.Equals(id));

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
            var assets = assetGuids.Select(s =>
                UnityEditor.AssetDatabase.LoadAssetAtPath<DataAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(s)));
            Assets = assets.ToArray();
        }
#endif
    }
}