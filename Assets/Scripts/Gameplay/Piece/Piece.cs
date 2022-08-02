using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay;
using InGame;
using SNM;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private ConfigData config;

    [Serializable]
    public class ConfigData
    {
        public int point;
        public Vector3 size;

        public Flocking.ConfigData flockingConfigData = new()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };
    }

    [field: NonSerialized] public PieceActivityQueue PieceActivityQueue { get; } = new();
    [field: NonSerialized] public PieceScheduler PieceScheduler { get; private set; }
    public ConfigData Config => config;
    private Transform _cameraTransform;

    private void Start()
    {
        if (Camera.main is not null) _cameraTransform = Camera.main.transform;
    }

    public void Setup()
    {
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        PieceScheduler = new PieceScheduler(this);
    }

    private void Update()
    {
        PieceActivityQueue?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new())
    {
        if (_cameraTransform == null) return;

        var t = transform;
        var dir = _cameraTransform.position - t.position;
        var up = t.up;
        dir = SNM.Math.Projection(dir, up);
        if (immediate)
        {
            transform.rotation = Quaternion.LookRotation(dir, up);
        }
        else
        {
            var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
            var duration = (target - transform.eulerAngles).magnitude / PieceActivityQueue.Config.angularSpeed;
            transform.DORotate(target, duration);
        }
    }

    public void JumpingMoveTo(Vector3 target)
    {
        // var flocking = new JumpingFlocking(
        //     config.flockingConfigData,
        //     new Flocking.InputData()
        //     {
        //         target = target,
        //         transform = transform
        //     }, null);
        var flocking = new Flocking(config.flockingConfigData,
            new Flocking.InputData {target = target, transform = transform}, null);
        flocking.AddVelocity(Vector3.one);
        PieceActivityQueue.Add(flocking);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Config == null) return;

        var t = transform;
        Gizmos.DrawWireCube(t.position + t.up * Config.size.y * 0.5f, Config.size);
    }
#endif
}