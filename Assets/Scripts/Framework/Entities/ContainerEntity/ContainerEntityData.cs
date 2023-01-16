using UnityEngine;
using System;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Services.Data;
using UnityEditor;

namespace Framework.Entities.ContainerEntity
{
    public interface IContainerEntityData : IEntityData
    {
        IEntityData[] GetComponentDataItems();
        string[] GetLoadAlongEntityIds();
    }

    public interface IContainerEntitySavedData : IEntitySavedData
    {
        IEntitySavedData[] GetComponentSavedDataItems();
        void SetComponentSavedDataItems(IEntitySavedData[] items);
    }

    public abstract class ContainerEntityData<TEntity> : EntityAsset<TEntity>, IContainerEntityData
        where TEntity : class, IContainerEntity<IContainerEntityData, IContainerEntitySavedData>
    {
        [SerializeField, ChildAsset(false), TypeConstraint(typeof(IEntityData))]
#if UNITY_EDITOR
        [ContextMenuItem(nameof(AddChildAssetsToComponents), nameof(AddChildAssetsToComponents))]
#endif
        private DataAsset[] componentAssets;

        [SerializeField, DataAssetIdSelector] private string[] loadAlongEntityIds;

        private IEntity<IEntityData, IEntitySavedData>[] CreateComponentEntityItems(IEntityLoader entityLoader)
        {
            var dataItems = GetComponentDataItems();
            var items = new IEntity<IEntityData, IEntitySavedData>[dataItems.Length];
            for (var i = 0; i < dataItems.Length; i++)
            {
                items[i] = dataItems[i].CreateEntity(entityLoader);
            }

            return items;
        }

        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            var components = CreateComponentEntityItems(entityLoader);
            var entity = CreateContainerEntityInternal();
            (entity as IContainerEntity<IContainerEntityData, IContainerEntitySavedData>)?.SetupInternal(components, loadAlongEntityIds.Select(entityLoader.CreateEntity).ToList());
            (entity.SavedData as IContainerEntitySavedData)?.SetComponentSavedDataItems(components.Select(i => i.SavedData).ToArray());
            return entity;
        }

        protected abstract IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal();
        public IEntityData[] GetComponentDataItems() => componentAssets.Select(d => d as IEntityData).ToArray();

        public string[] GetLoadAlongEntityIds()
        {
            return loadAlongEntityIds;
        }


#if UNITY_EDITOR
        [ContextMenuExtend(nameof(AddChildAssetsToComponents))]
        protected void AddChildAssetsToComponents()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this))
                .Where(a => a != this && !componentAssets.Contains(a) && a is DataAsset)
                .Select(a => a as DataAsset).ToArray();
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
    public class ContainerEntitySavedData<TEntityData> : BaseEntitySavedData<TEntityData>, IContainerEntitySavedData,
        IWriteToStorageCallbackHandler
        where TEntityData : IContainerEntityData
    {
        [SerializeField] private InnerAssetSavedDataService innerAssetSavedDataService = new();
        private IEntitySavedData[] _componentSavedDataItems;

        public ContainerEntitySavedData(TEntityData data) : base(data)
        {
            innerAssetSavedDataService.MarkedDirtyDelegate = OnInnerMarkedDirty;
        }

        private void OnInnerMarkedDirty()
        {
            Save();
        }

        public void SetComponentSavedDataItems(IEntitySavedData[] items)
        {
            _componentSavedDataItems = items;
        }

        public IEntitySavedData[] GetComponentSavedDataItems()
        {
            return _componentSavedDataItems;
        }

        public override void Load(ISavedDataService savedDataService)
        {
            base.Load(savedDataService);
            foreach (var item in _componentSavedDataItems)
            {
                item.Load(innerAssetSavedDataService);
            }
        }

        [Serializable]
        private class InnerAssetSavedDataService : ISavedDataService
        {
            [SerializeField] private InnerAssetSavedDataItem[] items = new InnerAssetSavedDataItem[0];

            public Action MarkedDirtyDelegate;

            public void Load(string savedId, object obj)
            {
                var index = Array.FindIndex(items, i => i.id.Equals(savedId));
                if (index == -1) return;
                JsonUtility.FromJsonOverwrite(items[index].json, obj);
                items[index].RuntimeObj = obj;
            }

            public void MarkDirty(string savedId, object obj)
            {
                var found = items.FirstOrDefault(i => i.id.Equals(savedId));
                if (found != null)
                {
                    found.RuntimeObj = obj;
                }
                else
                {
                    Array.Resize(ref items, items.Length + 1);
                    items[^1] = new InnerAssetSavedDataItem
                    {
                        id = savedId,
                        RuntimeObj = obj
                    };
                }

                MarkedDirtyDelegate?.Invoke();
            }

            public void WriteToStorage()
            {
                foreach (var i in items)
                {
                    (i.RuntimeObj as IWriteToStorageCallbackHandler)?.OnBeforeWrite();
                    i.json = JsonUtility.ToJson(i.RuntimeObj);
                }
            }
        }

        public void OnBeforeWrite()
        {
            innerAssetSavedDataService.WriteToStorage();
        }

        [Serializable]
        private class InnerAssetSavedDataItem
        {
            public string id;
            public object RuntimeObj;
            public string json;
        }
    }
}