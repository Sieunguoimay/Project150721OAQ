using UnityEngine;

namespace Common
{
    public class Singleton<TObject> : MonoBehaviour where TObject : MonoBehaviour
    {
        private static TObject _instance;

        public static TObject Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new GameObject($"(Singleton){nameof(TObject)}").AddComponent<TObject>();

                DontDestroyOnLoad(_instance.gameObject);

                return _instance;
            }
        }
    }
}