using System;
using Framework.Resolver;
using Framework.Services;
using UnityEngine;

namespace Framework
{
    public interface IInstaller
    {
        IResolver Resolver { get; }
    }

    public class MonoInstaller : MonoBehaviour, IInstaller
    {
        public static MonoInstaller Instance { get; private set; }
        
        private readonly IContainer _container = new Container();

        //Services
        private readonly IMessageService _messageService = new MessageService();

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
            _container.Bind(_messageService);
        }

        private void UnbindServices()
        {
            _container.Unbind(_messageService);
        }

        private void LoadEntities()
        {
        }

        private void UnloadEntities()
        {
            SaveEntities();
        }

        private void SaveEntities()
        {
        }

        public IResolver Resolver => _container;
    }
}