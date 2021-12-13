using UnityEngine;

namespace SNM
{
    public class PosAndRot
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public PosAndRot()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        public PosAndRot(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public PosAndRot(PosAndRot other)
        {
            Position = other.Position;
            Rotation = other.Rotation;
        }
    }
}