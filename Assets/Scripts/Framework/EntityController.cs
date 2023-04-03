using System;
using System.IO;
using Framework.Entities;
using Framework.Resolver;
using Framework.Services;
using Framework.Services.Data;
using Gameplay.Entities;
using UnityEngine;

namespace Framework
{
    public interface IInstaller
    {
        IResolver Resolver { get; }
    }

    public class EntityController : ScriptableObject, IInstaller
    {
        [SerializeField] private LocalDataServiceAsset dataServiceAsset;
        [SerializeField] private string saveFolder = "SavedData";

        [SerializeField, DataAssetIdSelector(typeof(IGameContentData))]
        private string gameContentId;

#if UNITY_EDITOR
        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        public string SavedDataPath => Path.Combine(ProjectPath, saveFolder);
#else
        //Todo:..
#endif

        public static IInstaller Instance { get; private set; }

        private readonly IContainer _container = new Container();
        public IResolver Resolver => _container;

        //Services

        private readonly EntityLoader _entityLoader = new();

        public void Load()
        {
            Instance = this;
            BindServices();
            InjectServices();
            LoadEntities();
        }

        private void BindServices()
        {
            _container.Bind<IBinder>(_container);
            _container.Bind<IMessageService>(new MessageService());
            _container.Bind<IDataService>(dataServiceAsset);
            _container.Bind<ISavedDataService>(new SavedDataService(SavedDataPath));
            _container.Bind<IEntityLoader>(_entityLoader);
        }

        private void InjectServices()
        {
            _entityLoader.Inject(Resolver);
        }
        
        private void LoadEntities()
        {
            LoadGameContent();
        }

        private void LoadGameContent()
        {
            var gameContent=_entityLoader.CreateEntity(gameContentId);
            gameContent.Inject(Resolver);
            gameContent.SetupDependencies();
        }
        
        public void Unload()
        {
            UnloadServices();
            UnloadEntities();
            UnbindServices();
        }
        
        public void UnloadServices()
        {
            _container.Resolve<ISavedDataService>().WriteToStorage();
        }
        
        private void UnloadEntities()
        {
            _entityLoader.DestroyEntity(_container.Resolve<IGameContent>(gameContentId));
        }

        private void UnbindServices()
        {
            _container.Unbind<IEntityLoader>();
            _container.Unbind<ISavedDataService>();
            _container.Unbind<IDataService>();
            _container.Unbind<IMessageService>();
            _container.Unbind<IBinder>();
        }
    }
}