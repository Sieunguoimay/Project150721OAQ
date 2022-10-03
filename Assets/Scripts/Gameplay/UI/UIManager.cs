using Common.ResolveSystem;
using UnityEngine;

namespace Gameplay
{
    public class UIManager : MonoBehaviour, IInjectable
    {
        public void Bind(IResolver resolver)
        {
        }

        public void Setup(IResolver resolver)
        {
            
        }

        public void TearDown()
        {
        }

        public void Unbind(IResolver resolver)
        {
        }
    }
}