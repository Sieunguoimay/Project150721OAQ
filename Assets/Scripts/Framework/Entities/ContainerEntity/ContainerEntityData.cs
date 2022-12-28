using Framework.Entities;
using UnityEngine;
using System;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Services.Data;
using Gameplay.Entities.MagicFlower;

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

    // [CreateAssetMenu(menuName = "Entity/ContainerEntityData")]
    public abstract class ContainerEntityData<TEntity> : EntityAsset<TEntity>, IContainerEntityData
        where TEntity : class, IContainerEntity<IContainerEntityData, IContainerEntitySavedData>
    {
        [SerializeField, ChildAsset(false), TypeConstraint(typeof(IEntityData))]
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
        // protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        // {
        //     var items = GetEntityItems();
        //     var savedData = new ContainerEntitySavedData(Id, items.Select(i => i.SavedData).ToArray());
        //     return new ContainerEntity<,>(this, savedData, items);
        // }

        public IEntityData[] GetComponentDataItems() => componentAssets.Select(d => d as IEntityData).ToArray();
    }

    [Serializable]
    public class ContainerEntitySavedData : BaseEntitySavedData, IContainerEntitySavedData
    {
        [SerializeField] private InnerAssetSavedDataService innerAssetSavedDataService = new();
        private readonly IEntitySavedData[] _componentSavedDataItems;

        public ContainerEntitySavedData(string id, IEntitySavedData[] componentSavedDataItems) : base(id)
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