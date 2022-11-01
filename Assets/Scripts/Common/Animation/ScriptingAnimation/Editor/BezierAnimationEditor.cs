using Common.Curve.Editor;
using UnityEditor;

namespace Common.Animation.ScriptingAnimation.Editor
{
    [CustomEditor(typeof(BezierAnimation))]
    public class BezierAnimationEditor : BezierSplineInspector
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            base.OnInspectorGUI();
        }
        
    }
}