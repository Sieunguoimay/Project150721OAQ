using System;
using Framework.Resolver;
using Framework.Services;
using Framework.Services.Data;
using UnityEngine;

namespace Framework.Entities
{
    public interface IEntity<out TData, out TSavedData> : IInjectable
        where TData : IEntityData where TSavedData : IEntitySavedData
    {
        TData Data { get; }
        TSavedData SavedData { get; }

        void Initialize();
        void Terminate();
    }

    public interface IEntityData
    {
        string Id { get; }
        IEntity<IEntityData, IEntitySavedData> CreateEntity();
        Type GetEntityType();
    }

    public interface IEntitySavedData
    {
        void Load(ISavedDataService savedDataService);
        void Save();
    }

    public class EntityData : IEntityData
    {
        public EntityData(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new BaseEntity<IEntityData, IEntitySavedData>(this, null);
        }

        public virtual Type GetEntityType()
        {
            return typeof(IEntityData);
        }
    }

    [Serializable]
    public abstract class BaseEntitySavedData : IEntitySavedData
    {
        [SerializeField] private string id;
        [NonSerialized] private ISavedDataService _savedDataService;

        protected BaseEntitySavedData(string id)
        {
            this.id = id;
        }

        public virtual void Load(ISavedDataService savedDataService)
        {
            _savedDataService = savedDataService;
            _savedDataService.Load(id, this);
            // var json = "{\"amount\":30.0}";
            // JsonUtility.FromJsonOverwrite(json, this);
        }

        public virtual void Save()
        {
            if (_savedDataService == null)
            {
                Debug.LogError($"SavedData {id} not Loaded");
                return;
            }

            _savedDataService.MarkDirty(id, this);
            // var json = JsonUtility.ToJson(this);
            // Debug.Log(json);
        }
    }

    public class BaseEntity<TData, TSavedData> : IEntity<TData, TSavedData>
        where TData : IEntityData
        where TSavedData : IEntitySavedData
    {
        private ISavedDataService _savedDataService;
        protected IResolver Resolver { get; private set; }

        public BaseEntity(TData data, TSavedData savedData)
        {
            Data = data;
            SavedData = savedData;
        }

        public virtual void Inject(IResolver resolver)
        {
            Resolver = resolver;
            _savedDataService = resolver.Resolve<ISavedDataService>();
        }

        public TData Data { get; }
        public TSavedData SavedData { get; }

        public virtual void Initialize()
        {
            SavedData?.Load(_savedDataService);
        }

        public virtual void Terminate()
        {
        }
    }
}