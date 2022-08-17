using System;
using SNM;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private System.Gameplay.GameplaySerializable gameplaySerializable;

    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; }= new();

    private System.Gameplay _gameplay;

    private void Awake()
    {
        Instance = this;
        if (!Instance)
        {
            Debug.LogError("Main: Error - not instantiatable");
        }

        _gameplay = new System.Gameplay(gameplaySerializable);
    }

    [ContextMenu("Setup")]
    private void Setup()
    {
        Debug.Log("Start");
        _gameplay.Setup();
    }
    private void OnDestroy()
    {
        _gameplay.TearDown();
    }

    private void Update()
    {
        if (!_gameplay.IsPlaying && Input.GetMouseButton(0))
        {
            _gameplay.StartNewMatch();
        }

        RayPointer.Update(Time.deltaTime);

        if (Input.GetKeyUp(KeyCode.Return))
        {
            _gameplay.ResetGame(this);
        }
    }
}