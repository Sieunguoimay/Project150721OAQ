using System;
using InGame.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestMain : MonoBehaviour
    {
        private PathMover _pathMover;

        private void Start()
        {
            _pathMover = (new GameObject(nameof(PathMover))).AddComponent<PathMover>();
            GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(_pathMover.transform);
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
            _pathMover.transform.position = Vector3.zero;
            _pathMover.FlyTo(Vector3.one * 5f);
        }

        [ContextMenu("Test2")]
        private void Test2()
        {
            _pathMover.transform.position = UnityEngine.Random.insideUnitSphere * 5f;
            _pathMover.SetPath(GetComponent<Path>());
        }
    }
}