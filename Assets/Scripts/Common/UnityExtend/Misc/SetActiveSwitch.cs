using UnityEngine;

namespace Common.UnityExtend.Misc
{
    public class SetActiveSwitch : MonoBehaviour
    {
        [SerializeField] private GameObject[] gameObjects;

        public void SetActive(int index)
        {
            for (var i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(index == i);
            }
        }
    }
}