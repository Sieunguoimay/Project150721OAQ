using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private Config config;

    [Serializable]
    public class Config
    {
        [SerializeField] private GameObject boardPrefab;
        [SerializeField] private GameObject tileSelector;
        [SerializeField] private GameObject dronePrefab;
        public GameObject BoardPrefab => boardPrefab;
        public GameObject TileSelector => tileSelector;
        public GameObject DronePrefab => dronePrefab;
    }

    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; private set; }

    private GameController _controller;

    private void Awake()
    {
        Instance = this;
        if (!Instance)
        {
            Debug.LogError("Main: Error - not instantiatable");
        }

        RayPointer = new RayPointer();
        RayPointer.Reset();
        _controller = new GameController(config);
    }

    void Start()
    {
        _controller.Setup();

        this.Delay(1f, _controller.StartNewMatch);
    }

    private void Update()
    {
        RayPointer.Update(Time.deltaTime);

        if (_controller.IsGameOver)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                _controller.ResetGame(this);
            }
        }
    }
}