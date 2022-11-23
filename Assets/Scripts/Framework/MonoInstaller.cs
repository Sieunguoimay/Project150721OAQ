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
        bool Done { get; }
    }

    public class MonoInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private LocalDataServiceAsset dataServiceAsset;
        [SerializeField] private string saveFolder = "SavedData";
        [SerializeField,IdSelector(typeof(IGameContentData))] private string gameContentId;

#if UNITY_EDITOR
        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        public string SavedDataPath => Path.Combine(ProjectPath, saveFolder);
#else
#endif

        public static IInstaller Instance { get; private set; }

        private readonly IContainer _container = new Container();

        //Services

        private readonly IEntityLoader _entityLoader = new EntityLoader();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void Start()
        {
            BindServices();
            LoadEntities();
            FindObjectOfType<Launcher>()?.LoadGameScene();
            Done = true;
        }

        private void OnDestroy()
        {
            UnloadEntities();
            UnbindServices();
        }

        private void OnApplicationQuit()
        {
            SaveEntities();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
            }
            else
            {
                SaveEntities();
            }
        }

        private void BindServices()
        {
            _container.Bind<IBinder>(_container);
            _container.Bind<IMessageService>(new MessageService());
            _container.Bind<IDataService>(dataServiceAsset);
            _container.Bind<ISavedDataService>(new SavedDataService(SavedDataPath));
            _container.Bind<IEntityLoader>(_entityLoader);
        }

        private void UnbindServices()
        {
            _container.Unbind<IEntityLoader>();
            _container.Unbind<ISavedDataService>();
            _container.Unbind<IDataService>();
            _container.Unbind<IMessageService>();
            _container.Unbind<IBinder>();
        }

        private void LoadEntities()
        {
            _entityLoader.Inject(_container);
            _entityLoader.CreateEntity(gameContentId);
        }

        private void UnloadEntities()
        {
            SaveEntities();
            _entityLoader.DestroyEntity(_container.Resolve<IGameContent>(gameContentId));
        }

        private void SaveEntities()
        {
            _container.Resolve<ISavedDataService>().WriteToStorage();
        }

        public IResolver Resolver => _container;
        [field: NonSerialized] public bool Done { get; private set; } = false;
    }
}