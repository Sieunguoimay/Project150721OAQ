﻿using System;
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

    [CreateAssetMenu(menuName = "Controller/EntityController")]
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
#endif

        public static IInstaller Instance { get; private set; }

        private readonly IContainer _container = new Container();

        //Services

        private readonly EntityLoader _entityLoader = new();

        public void Load()
        {
            Instance = this;
            BindServices();
            LoadEntities();
        }

        public void Unload()
        {
            UnloadEntities();
            UnbindServices();
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
            _entityLoader.SetupEntities();
        }

        private void UnloadEntities()
        {
            SaveEntities();
            _entityLoader.DestroyEntity(_container.Resolve<IGameContent>(gameContentId));
        }

        public void SaveEntities()
        {
            _container.Resolve<ISavedDataService>().WriteToStorage();
        }

        public IResolver Resolver => _container;
    }
}