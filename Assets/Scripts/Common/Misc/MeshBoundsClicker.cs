using UnityEngine;

namespace Common.Misc
{
    [RequireComponent(typeof(Renderer))]
    public class MeshBoundsClicker : ABoundsClicker
    {
        private Renderer _renderer;

        public override Bounds Bounds
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<Renderer>();
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