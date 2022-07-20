using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class TransformableMono : AMonoBehaviourWrapper<ITransformable>
    {
        public override ITransformable Create()
        {
            return new Transformable(transform);
        }
    }
}