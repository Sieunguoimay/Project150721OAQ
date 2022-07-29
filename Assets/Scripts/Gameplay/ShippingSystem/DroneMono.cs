using Implementations.Transporter;
public class DroneMono : TransporterMono //, ISplineModifier
{
    // [SerializeField] private UnityEngine.Object splineConfig;
    //
    // private ISplineModifier _splineModifier;
    // private BezierSpline _bezierSpline;
    // private BezierMover _bezierMover;
    // private int _step;
    // private PosAndRot _target;
    // private Transform _prevParent;
    // private IDronePackage _objectTransform;
    // private Vector3 _initialPosition;
    //
    // private void Awake()
    // {
    //     _splineModifier = splineConfig is GameObject
    //         ? (splineConfig as GameObject)?.GetComponent<ISplineModifier>()
    //         : (splineConfig as ISplineModifier);
    //     _bezierSpline = new GameObject(nameof(BezierSpline)).AddComponent<BezierSpline>();
    //     _bezierSpline.Reset();
    //     _splineModifier.Setup(_bezierSpline);
    //     _bezierMover = GetComponent<BezierMover>();
    //     _bezierMover.ChangePath(_bezierSpline);
    //     _bezierMover.OnComplete -= BezierMover_OnComplete;
    //     _bezierMover.OnComplete += BezierMover_OnComplete;
    //     transform.position += Vector3.up * 2f;
    //     _initialPosition = transform.position;
    // }
    //
    // public void GraspObjectToTarget(IDronePackage package, PosAndRot pos)
    // {
    //     _objectTransform = package;
    //     _target = pos;
    //     _step = 0;
    //
    //     var pickupPoint = _objectTransform.GetTransform().position + _objectTransform.GetPickupPoint();
    //     _splineModifier.Modify(transform.position, pickupPoint);
    //     _bezierMover.Move(Vector3.Distance(transform.position, pickupPoint) /
    //                       _bezierMover.GetConfig().Speed);
    // }
    //
    // private void BezierMover_OnComplete()
    // {
    //     var t = _objectTransform.GetTransform();
    //     if (_step == 0)
    //     {
    //         _prevParent = t.parent;
    //         t.SetParent(transform);
    //         t.position = t.position;
    //         t.rotation = t.rotation;
    //
    //         _splineModifier.Modify(transform.position, _target.Position + Vector3.up);
    //         _bezierMover.Move(Vector3.Distance(transform.position, _target.Position + Vector3.up) /
    //                           _bezierMover.GetConfig().Speed);
    //         _step++;
    //     }
    //     else
    //     {
    //         t.SetParent(_prevParent);
    //         t.SetPositionAndRotation(_target.Position, _target.Rotation);
    //
    //         _splineModifier.Modify(transform.position, _initialPosition);
    //         _bezierMover.Move(Vector3.Distance(transform.position, _initialPosition) /
    //                           _bezierMover.GetConfig().Speed);
    //     }
    // }
    //
    // public void Setup(BezierSpline spline)
    // {
    // }
    //
    // public void Modify(Vector3 p0, Vector3 p1)
    // {
    //     _bezierSpline.SetPoint(0, p0);
    //     _bezierSpline.SetPoint(1, p0 + (p1 - p0) * 0.3f);
    //     _bezierSpline.SetPoint(3, p1);
    //     _bezierSpline.SetPoint(2, p1 + Vector3.up * (Vector3.Distance(p0, p1) * 0.3f));
    // }
}