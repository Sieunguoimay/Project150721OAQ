using System;
using InGame.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestMain : MonoBehaviour
    {
        [SerializeField, Range(-360, 360)] private float localEulerAngleX;
        [SerializeField, Range(-360, 360)] private float localEulerAngleY;
        [SerializeField, Range(-360, 360)] private float localEulerAngleZ;

        [SerializeField, Range(-10, 10)] private float quaternionX;
        [SerializeField, Range(-10, 10)] private float quaternionY;
        [SerializeField, Range(-10, 10)] private float quaternionZ;
        [SerializeField, Range(-10, 10)] private float quaternionW;

        private void OnValidate()
        {
            // transform.localEulerAngles = new Vector3(localEulerAngleX, localEulerAngleY, localEulerAngleZ);
            // transform.rotation = ToQuaternion(Mathf.Deg2Rad * localEulerAngleX, Mathf.Deg2Rad * localEulerAngleY, Mathf.Deg2Rad * localEulerAngleZ);
            transform.rotation= Quaternion.Euler(Vector3.right* localEulerAngleX);
        }

        // private PathMover _pathMover;
        //
        // private void Start()
        // {
        //     _pathMover = (new GameObject(nameof(PathMover))).AddComponent<PathMover>();
        //     GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(_pathMover.transform);
        // }
        //
        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Return))
        //     {
        //         Test();
        //     }
        // }
        //
        // [ContextMenu("Test")]
        // private void Test()
        // {
        //     _pathMover.transform.position = Vector3.zero;
        //     _pathMover.FlyTo(Vector3.one * 5f);
        // }
        //
        // [ContextMenu("Test2")]
        // private void Test2()
        // {
        //     _pathMover.transform.position = UnityEngine.Random.insideUnitSphere * 5f;
        //     _pathMover.SetPath(GetComponent<Path>());
        // }


        Quaternion ToQuaternion(float roll, float pitch, float yaw) // roll (x), pitch (Y), yaw (z)
        {
            // Abbreviations for the various angular functions

            var cr = Mathf.Cos(roll * 0.5f);
            var sr = Mathf.Sin(roll * 0.5f);
            var cp = Mathf.Cos(pitch * 0.5f);
            var sp = Mathf.Sin(pitch * 0.5f);
            var cy = Mathf.Cos(yaw * 0.5f);
            var sy = Mathf.Sin(yaw * 0.5f);

            Quaternion q;
            q.w = cr * cp * cy + sr * sp * sy;
            q.x = sr * cp * cy - cr * sp * sy;
            q.y = cr * sp * cy + sr * cp * sy;
            q.z = cr * cp * sy - sr * sp * cy;

            return q;
        }
    }
}