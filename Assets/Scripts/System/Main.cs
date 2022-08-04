using System;
using SNM;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private System.Gameplay.GameplaySerializable gameplaySerializable;

    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; private set; }

    private System.Gameplay _gameplay;

    private void Awake()
    {
        Instance = this;
        if (!Instance)
        {
            Debug.LogError("Main: Error - not instantiatable");
        }

        RayPointer = new RayPointer();
        RayPointer.Reset();
        _gameplay = new System.Gameplay(gameplaySerializable);
    }

    private void Start()
    {
        Debug.Log("Start");
        _gameplay.Setup();

        this.Delay(1f, _gameplay.StartNewMatch);
    }

    private void Update()
    {
        RayPointer.Update(Time.deltaTime);

        if (!_gameplay.IsGameOver) return;

        if (Input.GetKeyUp(KeyCode.Return))
        {
            _gameplay.ResetGame(this);
        }
    }
}