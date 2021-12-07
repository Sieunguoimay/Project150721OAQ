using System;
using InGame.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestMain : MonoBehaviour
    {
        private FlyingObject _flyingObject;

        private void Start()
        {
            _flyingObject = (new GameObject(nameof(FlyingObject))).AddComponent<FlyingObject>();
            GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(_flyingObject.transform);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Test();
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            _flyingObject.transform.position = Vector3.zero;
            _flyingObject.FlyTo(Vector3.one * 5f);
        }

        [ContextMenu("Test2")]
        private void Test2()
        {
            _flyingObject.transform.position = UnityEngine.Random.insideUnitSphere * 5f;
            _flyingObject.SetPath(GetComponent<Path>());
        }
    }
}