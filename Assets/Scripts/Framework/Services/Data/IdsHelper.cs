using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Framework.Services.Data
{
#if UNITY_EDITOR
    public static class IdsHelper
    {
        private static Dictionary<Type, List<string>> _ids;

        public static IEnumerable<string> GetIds(Type typeConstraint)
        {
            if (_ids == null)
            {
                ForceUpdateIds();
            }

            if (_ids == null) return null;

            if (typeConstraint == null)
            {
                return _ids.SelectMany(a => a.Value).ToList();
            }

            if (_ids.TryGetValue(typeConstraint, out var output))
            {
                return output;
            }

            return null;
        }

        [MenuItem("Tools/IdsHelper/Force Update Ids")]
        public static void ForceUpdateIds()
        {
            var validTypes = Assembly
                .GetAssembly(typeof(DataAsset))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DataAsset))).SelectMany(t=>t.GetInterfaces()).Distinct();
            var assetGuids = AssetDatabase.FindAssets($"t: {typeof(DataAsset).FullName}");
            var assets = assetGuids.Select(s => AssetDatabase.LoadAssetAtPath<DataAsset>(AssetDatabase.GUIDToAssetPath(s))).ToList();

            _ids = new Dictionary<Type, List<string>>();
            
            foreach (var validType in validTypes)
            {
                _ids.Add(validType, assets.Where(a => validType.IsInstanceOfType(a)).Select(asset => asset.Id).ToList());
            }
        }
    }
#endif
}