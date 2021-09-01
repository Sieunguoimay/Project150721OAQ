using UnityEngine;

namespace SNM
{
    public class Placement
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Placement()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        public Placement(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Placement(Placement other)
        {
            Position = other.Position;
            Rotation = other.Rotation;
        }
    }
}