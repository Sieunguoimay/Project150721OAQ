using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        public Boid.ConfigData boidConfigData = new Boid.ConfigData()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };
    }

    public PieceActor PieceActor { get; } = new PieceActor();
    public PieceScheduler PieceScheduler { get; private set; }
    public ConfigData Config => config;
    // public Transform FootTransform { get; private set; }
    //
    // private Tag[] _taggedGameObjects;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void Setup()
    {
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        // _taggedGameObjects = GetComponentsInChildren<Tag>();
        //
        // FootTransform = _taggedGameObjects.FirstOrDefault(t => t.ID.Equals("foot"))?.transform;

        PieceScheduler = new PieceScheduler(this);
    }

    private void Update()
    {
        PieceActor?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new Vector3())
    {
        if (_camera != null)
        {
            var t = transform;
            var dir = _camera.transform.position - t.position;
            var up = t.up;
            dir = SNM.Math.Projection(dir, up);
            if (immediate)
            {
                transform.rotation = Quaternion.LookRotation(dir, up);
            }
            else
            {
                var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
                var duration = (target - transform.eulerAngles).magnitude / PieceActor.Config.angularSpeed;
                transform.DORotate(target, duration);
            }
        }
    }

    public void JumpingMoveTo(Vector3 target)
    {
        var newBoid = new JumpingBoid(
            config.boidConfigData,
            new Boid.InputData()
            {
                target = target,
                transform = transform
            }, null);
        PieceActor.Add(newBoid);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Config != null)
        {
            Gizmos.DrawWireCube(transform.position + transform.up * Config.size.y * 0.5f, Config.size);
        }
    }
#endif
}