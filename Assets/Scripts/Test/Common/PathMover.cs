using System;
using System.Collections;
using System.Collections.Generic;
using InGame.Common;
using UnityEngine;
using UnityEngine.Serialization;

public class PathMover : MonoBehaviour
{
    [FormerlySerializedAs("speed")] [SerializeField] private float desiredSpeed = 0.1f;

    private Vector3 _vel;
    private Vector3 _target;

    private Path _path;


    private void Update()
    {
        if (_path)
        {
            Follow(_path);
        }

        var t = transform;
        var pos = t.position;
        pos += _vel;
        t.position = pos;
    }

    public void SetPath(Path path) => _path = path;

    public void FlyTo(Vector3 b)
    {
        _target = b;
    }

    protected virtual Vector3 GetVelocity()
    {
        var diff = _target - transform.position;
        var s = diff.sqrMagnitude;

        if (s > desiredSpeed)
        {
            return diff / Mathf.Sqrt(s) * desiredSpeed;
        }

        return Vector3.zero;
    }

    private void Follow(Path path)
    {
        var predict = _vel;
        predict = predict.normalized * desiredSpeed;
        var predictLoc = transform.position + predict;

        if (path.points.Count <= 1) return;

        var minDistance = float.MaxValue;
        for (var i = 0; i < path.points.Count - 1; i++)
        {
            var a = path.points[i];
            var b = path.points[i + 1];
            var normalPoint = GetNormalPoint(predictLoc, a, b);

            var distance = Vector3.Distance(normalPoint, predictLoc);
            if (distance < minDistance)
            {
                minDistance = distance;
                _target = normalPoint + (b - a).normalized * 2f;
            }
        }

        Seek(_target);
    }

    private static Vector3 GetNormalPoint(Vector3 predictLoc, Vector3 a, Vector3 b)
    {
        var ap = predictLoc - a;
        var ab = (b - a);
        var abSqrMag = ab.sqrMagnitude;
        var abNor = ab / Mathf.Sqrt(abSqrMag);
        var len = Vector3.Dot(ap, abNor);
        var normalPoint = a + abNor * len;

        if (len < 0 || len * len > abSqrMag)
        {
            normalPoint = b;
        }

        return normalPoint;
    }

    private void Seek(Vector3 target)
    {
        var diff = target - transform.position;
        var s = diff.sqrMagnitude;

        if (s > desiredSpeed)
        {
            _vel = diff.normalized * desiredSpeed;
        }
    }

    private void OnDrawGizmos()
    {
        var pos = transform.position;
        Gizmos.DrawLine(pos, pos + _vel * 10f);
        Gizmos.DrawCube(_target, Vector3.one * 0.25f);
    }
}