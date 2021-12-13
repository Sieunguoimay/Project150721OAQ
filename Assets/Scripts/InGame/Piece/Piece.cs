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
    }
    public PieceActor PieceActor { get; } = new PieceActor();
    public PieceScheduler PieceScheduler { get; private set; }
    public ConfigData Config => config;
    public Transform FootTransform { get; private set; }

    private Tag[] _taggedGameObjects;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void Setup()
    {
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        _taggedGameObjects = GetComponentsInChildren<Tag>();

        FootTransform = _taggedGameObjects.FirstOrDefault(t => t.ID.Equals("foot"))?.transform;

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
            var dir = _camera.transform.position - transform.position;
            var up = transform.up;
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


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Config != null)
        {
            Gizmos.DrawWireCube(transform.position, Config.size);
        }
    }
#endif

}