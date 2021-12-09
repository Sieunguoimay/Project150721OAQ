using UnityEngine;

namespace SNM
{
    public class LinearTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public LinearTransform()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        public LinearTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public LinearTransform(LinearTransform other)
        {
            Position = other.Position;
            Rotation = other.Rotation;
        }
    }
}