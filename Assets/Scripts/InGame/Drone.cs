using System;
using UnityEngine;
using System.Collections;
using SNM;
using SNM.Bezier;

public class Drone : MasterComponent
{

    private Actor _actor;

    //references
    private IPickedUpObject _pickedUpObject;
    private Placement _targetPlacement;
    private Transform _endPoint;

    public void Setup(Transform endPoint)
    {
        _actor = new Actor();
        transform.position = UnityEngine.Random.insideUnitSphere + Vector3.up;
        _endPoint = endPoint;
        SetupReferences();
    }

    public void Loop(float deltaTime)
    {
        _actor.Update(deltaTime);
    }

    public void GraspObjectToTarget(IPickedUpObject pickedUpObject, Placement targetPlacement)
    {
        _pickedUpObject = pickedUpObject;
        _targetPlacement = targetPlacement;
        _actor.Add(new BezierMotionActivity(_bezierMotion, 0.5f, this._pickedUpObject.Transform.position));
        _actor.Add(new PickUp(this, _pickedUpObject));
        _actor.Add(new BezierMotionDropActivity(_pickedUpObject,
            Main.Instance.transform, _bezierMotion, _targetPlacement, _endPoint));
    }

    public void Cleanup()
    {
        _pickedUpObject = null;
    }

    private void SetupReferences()
    {
        _bezierMotion = GetComponent<BezierMotion>();
    }

    private BezierMotion _bezierMotion;

    public interface IPickedUpObject
    {
        void OnPick(Transform attachTarget);
        void OnDrop(Transform oldParent, Placement targetPlacement);
        Transform Transform { get; }
    }

    private class PickUp : Activity
    {
        private Drone visual;
        private IPickedUpObject pickedUpObject;

        public PickUp(Drone visual, IPickedUpObject pickedUpObject)
        {
            this.visual = visual;
            this.pickedUpObject = pickedUpObject;
        }

        public override void Update(float deltaTime)
        {
            pickedUpObject.OnPick(visual.transform);
            IsDone = true;
        }
    }

    private class BezierMotionActivity : Activity, BezierMotion.IListener
    {
        private readonly BezierMotion _bezierMotion;

        private readonly Transform _transform;
        private readonly Vector3 _targetPoint;
        private int _listenerId;

        public BezierMotionActivity(BezierMotion bezierMotion, float dropPoint, Vector3 targetPoint)
        {
            _bezierMotion = bezierMotion;
            _transform = bezierMotion.GetComponent<Transform>();
            _targetPoint = targetPoint;
        }

        public override void Begin()
        {
            var points = new Vector3[3];
            var position = _transform.position;

            points[0] = position;
            points[1] = position + (_targetPoint - position) * 0.75f + Vector3.up;
            points[2] = _targetPoint;
            _bezierMotion.RegisterListener(this, 0f);
            _bezierMotion.Move(points);
        }

        public override void End()
        {
            _bezierMotion.UnregisterListener(this);
        }

        public void OnBegin()
        {
        }

        public void OnThresholdExceeded()
        {
        }

        public void OnEnd()
        {
            IsDone = true;
        }

        public int ID { get; set; }
    }

    private class BezierMotionDropActivity : Activity, BezierMotion.IListener
    {
        private readonly BezierMotion _bezierMotion;
        private IPickedUpObject _pickedUpObject;
        private Transform _newParent;
        private int _listenerId;
        private float _dropPoint;
        private Placement _targetPlacement;

        private Vector3[] _points;
        private readonly Transform _endPoint;

        public BezierMotionDropActivity(IPickedUpObject pickedUpObject, Transform newParent,
            BezierMotion bezierMotion,
            Placement targetPlacement,
            Transform endPoint)
        {
            _pickedUpObject = pickedUpObject;
            _bezierMotion = bezierMotion;
            _newParent = newParent;
            _targetPlacement = targetPlacement;
            _endPoint = endPoint;
        }

        public override void Update(float deltaTime)
        {
        }

        public override void Begin()
        {
            var pos = _bezierMotion.transform.position;
            var diff = _targetPlacement.Position - pos;
            var perpend = Vector3.Cross(diff, Vector3.up);

            _points = new Vector3[5];
            _points[0] = pos;
            _points[1] = pos + perpend * 2f;
            _points[3] = pos + diff * 0.1f;
            _points[2] = pos - perpend * 2f;
            _points[4] = _endPoint.position;

            _dropPoint = BezierPlotter.CalculateT(_points, _targetPlacement.Position);

            _bezierMotion.RegisterListener(this, _dropPoint);
            _bezierMotion.Move(_points);
        }

        public override void End()
        {
            _bezierMotion.UnregisterListener(this);
        }


        public void OnBegin()
        {
            Debug.Log("Dropper onbegin");
        }

        public void OnThresholdExceeded()
        {
            _pickedUpObject.OnDrop(_newParent, _targetPlacement);
            Debug.Log("Dropper OnThresholdExceeded");
        }

        public void OnEnd()
        {
            IsDone = true;
            Debug.Log("Dropper OnEnd");
        }

        public int ID { get; set; }
    }
}