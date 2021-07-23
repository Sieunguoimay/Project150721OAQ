using UnityEngine;

namespace SNM
{
    public class Utils
    {
        public static T NewGameObject<T>() where T : MonoBehaviour
        {
            return (new GameObject(nameof(T))).AddComponent<T>();
        }
    }
}