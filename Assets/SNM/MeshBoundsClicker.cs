using UnityEngine;

namespace SNM
{
    [RequireComponent(typeof(Renderer))]
    public class MeshBoundsClicker : ABoundsClicker
    {
        private Renderer _renderer;
        public override Bounds Bounds
        {
            get
            {
                if (!Application.isPlaying)
                {
                    if (_renderer == null)
                    {
                        _renderer = GetComponent<MeshRenderer>();
                    }
                }
                return _renderer.bounds;
            }
        }

        protected override void InnerSetup()
        {
            _renderer = GetComponent<Renderer>();
            base.InnerSetup();
        }
    }
}