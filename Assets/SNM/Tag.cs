using UnityEngine;

namespace SNM
{
    public class Tag : MonoBehaviour
    {
        [SerializeField] private string id;

        public string ID => id;
    }
}