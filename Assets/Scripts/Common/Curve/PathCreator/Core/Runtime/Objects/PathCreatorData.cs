using UnityEngine;

namespace Common.Curve.PathCreator.Core.Runtime.Objects {
    /// Stores state data for the path creator editor

    [System.Serializable]
    public class PathCreatorData {
        public event System.Action BezierOrVertexPathModified;
        public event System.Action BezierCreated;

        [SerializeField] private BezierPath bezierPath;

        private VertexPath _vertexPath;

        [SerializeField] private bool vertexPathUpToDate;

        // vertex path settings
        public float vertexPathMaxAngleError = .3f;
        public float vertexPathMinVertexSpacing = 0.01f;

        // bezier display settings
        public bool showTransformTool = true;
        public bool showPathBounds;
        public bool showPerSegmentBounds;
        public bool displayAnchorPoints = true;
        public bool displayControlPoints = true;
        public float bezierHandleScale = 1;
        public bool globalDisplaySettingsFoldout;
        public bool keepConstantHandleSize;

        // vertex display settings
        public bool showNormalsInVertexMode;
        public bool showBezierPathInVertexMode;

        // Editor display states
        public bool showDisplayOptions;
        public bool showPathOptions = true;
        public bool showVertexPathDisplayOptions;
        public bool showVertexPathOptions = true;
        public bool showNormals;
        public bool showNormalsHelpInfo;
        public int tabIndex;

        public void Initialize (bool defaultIs2D) {
            if (bezierPath == null) {
                CreateBezier (Vector3.zero, defaultIs2D);
            }
            vertexPathUpToDate = false;
            bezierPath.OnModified -= BezierPathEdited;
            bezierPath.OnModified += BezierPathEdited;
        }

        public void ResetBezierPath (Vector3 centre, bool defaultIs2D = false) {
            CreateBezier (centre, defaultIs2D);
        }

        private void CreateBezier (Vector3 centre, bool defaultIs2D = false) {
            if (bezierPath != null) {
                bezierPath.OnModified -= BezierPathEdited;
            }

            var space = (defaultIs2D) ? PathSpace.XY : PathSpace.XYZ;
            bezierPath = new BezierPath (centre, false, space);

            bezierPath.OnModified += BezierPathEdited;
            vertexPathUpToDate = false;

            if (BezierOrVertexPathModified != null) {
                BezierOrVertexPathModified ();
            }
            if (BezierCreated != null) {
                BezierCreated ();
            }
        }

        public BezierPath BezierPath {
            get => bezierPath;
            set {
                bezierPath.OnModified -= BezierPathEdited;
                vertexPathUpToDate = false;
                bezierPath = value;
                bezierPath.OnModified += BezierPathEdited;

                if (BezierOrVertexPathModified != null) {
                    BezierOrVertexPathModified ();
                }
                if (BezierCreated != null) {
                    BezierCreated ();
                }

            }
        }

        // Get the current vertex path
        public VertexPath GetVertexPath (Transform transform) {
            // create new vertex path if path was modified since this vertex path was created
            if (!vertexPathUpToDate || _vertexPath == null) {
                vertexPathUpToDate = true;
                _vertexPath = new VertexPath (BezierPath, transform, vertexPathMaxAngleError, vertexPathMinVertexSpacing);
            }
            return _vertexPath;
        }

        public void PathTransformed () {
            if (BezierOrVertexPathModified != null) {
                BezierOrVertexPathModified ();
            }
        }

        public void VertexPathSettingsChanged () {
            vertexPathUpToDate = false;
            if (BezierOrVertexPathModified != null) {
                BezierOrVertexPathModified ();
            }
        }

        public void PathModifiedByUndo () {
            vertexPathUpToDate = false;
            if (BezierOrVertexPathModified != null) {
                BezierOrVertexPathModified ();
            }
        }

        void BezierPathEdited () {
            vertexPathUpToDate = false;
            if (BezierOrVertexPathModified != null) {
                BezierOrVertexPathModified ();
            }
        }

    }
}