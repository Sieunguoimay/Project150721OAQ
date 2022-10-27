using System;
using Framework.Entities;
using Framework.Resolver;
using Framework.Services;
using Gameplay.Entities;
using UnityEngine;

namespace Framework
{
    public interface IInstaller
    {
        IResolver Resolver { get; }
    }

    public class MonoInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private LocalDataServiceAsset dataServiceAsset;
        public static IInstaller Instance { get; private set; }

        private readonly IContainer _container = new Container();

        //Services
        private readonly IMessageService _messageService = new MessageService();

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
            FindObjectOfType<Launcher>().LoadGameScene();
        }

        private void OnDestroy()
        {
            UnloadEntities();
            UnbindServices();
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
            _container.Bind<IMessageService>(_messageService);
            _container.Bind<IDataService>(dataServiceAsset);
            _container.Bind<IEntityLoader>(_entityLoader);
        }

        private void UnbindServices()
        {
            _container.Unbind<IEntityLoader>(dataServiceAsset);
            _container.Unbind<IDataService>(dataServiceAsset);
            _container.Unbind<IMessageService>(_messageService);
            _container.Unbind<IBinder>(_container);
        }

        private void LoadEntities()
        {
            _entityLoader.Inject(_container);
            _entityLoader.CreateEntity<IGameContent, IGameContentData>("game_content");
        }

        private void UnloadEntities()
        {
            SaveEntities();
            _entityLoader.DestroyEntity<IGameContent>("game_content");
        }

        private void SaveEntities()
        {
            // _container.Resolve<ICurrency>("game_currency").SavedData
        }

        public IResolver Resolver => _container;
    }
}