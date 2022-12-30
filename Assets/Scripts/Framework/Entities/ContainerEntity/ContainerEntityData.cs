using Framework.Entities;
using UnityEngine;
using System;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Services.Data;
using Gameplay.Entities.MagicFlower;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Framework.Entities.ContainerEntity
{
    public interface IContainerEntityData : IEntityData
    {
        IEntityData[] GetComponentDataItems();
    }

    public interface IContainerEntitySavedData : IEntitySavedData
    {
        IEntitySavedData[] GetComponentSavedDataItems();
    }

    public abstract class ContainerEntityData<TEntity> : EntityAsset<TEntity>, IContainerEntityData
        where TEntity : class, IContainerEntity<IContainerEntityData, IContainerEntitySavedData>
    {
        [SerializeField, ChildAsset(false), TypeConstraint(typeof(IEntityData))]
        [ContextMenuItem(nameof(AddChildAssetsToComponents), nameof(AddChildAssetsToComponents))]
        private DataAsset[] componentAssets;

        protected IEntity<IEntityData, IEntitySavedData>[] GetEntityItems()
        {
            var dataItems = GetComponentDataItems();
            var items = new IEntity<IEntityData, IEntitySavedData>[dataItems.Length];
            for (var i = 0; i < dataItems.Length; i++)
            {
                items[i] = dataItems[i].CreateEntity();
            }

            return items;
        }

        public IEntityData[] GetComponentDataItems() => componentAssets.Select(d => d as IEntityData).ToArray();

#if UNITY_EDITOR
        [ContextMenuExtend(nameof(AddChildAssetsToComponents))]
        protected void AddChildAssetsToComponents()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)).Where(a => a != this && !componentAssets.Contains(a) && a is DataAsset).Select(a => a as DataAsset).ToArray();
            if (assets.Length <= 0) return;
            Array.Resize(ref componentAssets, componentAssets.Length + assets.Length);
            for (var i = 0; i < assets.Length; i++)
            {
                componentAssets[i] = assets[i];
            }
        }

#endif
    }

    [Serializable]
    public class ContainerEntitySavedData<TEntityData> : BaseEntitySavedData<TEntityData>, IContainerEntitySavedData
    where TEntityData: IContainerEntityData
    {
        [SerializeField] private InnerAssetSavedDataService innerAssetSavedDataService = new();
        private readonly IEntitySavedData[] _componentSavedDataItems;

        public ContainerEntitySavedData(TEntityData data, IEntitySavedData[] componentSavedDataItems) : base(data)
        {
            _componentSavedDataItems = componentSavedDataItems;
        }

        public IEntitySavedData[] GetComponentSavedDataItems()
        {
            return _componentSavedDataItems;
        }

        public override void Load(ISavedDataService savedDataService)
        {
            base.Load(savedDataService);
            foreach (var item in GetComponentSavedDataItems())
            {
                item.Load(innerAssetSavedDataService);
            }
        }

        [Serializable]
        private class InnerAssetSavedDataService : ISavedDataService
        {
            [SerializeField] private InnerAssetSavedDataItem[] items = new InnerAssetSavedDataItem[0];

            public void Load(string savedId, object obj)
            {
                var found = items?.FirstOrDefault(i => i.id.Equals(savedId));
                if (found != null)
                {
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(found.Obj), obj);
                }
            }

            public void MarkDirty(string savedId, object obj)
            {
                var found = items.Any(i => i.id.Equals(savedId));
                if (found) return;

                Array.Resize(ref items, items.Length + 1);
                items[^1] = new InnerAssetSavedDataItem
                {
                    id = savedId,
                    Obj = obj
                };
            }

            public void WriteToStorage()
            {
                //Not used
            }
        }

        [Serializable]
        private class InnerAssetSavedDataItem
        {
            public string id;
            public object Obj;
        }
    }
}