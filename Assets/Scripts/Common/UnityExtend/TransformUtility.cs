using UnityEngine;

namespace Common.UnityExtend
{
    public static class TransformUtility
    {
        public static void CopyWorldScale(Transform to, Transform from)
        {
            var oldParent = from.parent;
            from.SetParent(to.parent, true);
            to.localScale = from.localScale;
            from.SetParent(oldParent, true);
        }
    }
}