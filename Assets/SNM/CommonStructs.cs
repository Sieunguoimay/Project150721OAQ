using UnityEngine;

namespace SNM
{
    public class PosAndRot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public PosAndRot(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}