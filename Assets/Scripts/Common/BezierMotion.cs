using System;
using System.Collections.Generic;
using SNM;
using SNM.Bezier;
using SNM.EventSystem;
using UnityEngine;

public class BezierMotion : MonoBehaviour, TimeRunner.IListener
{
    private Vector3[] _points = null;
    private TimeRunner _timeRunner;
    private readonly List<ListenerData> _listenersData = new List<ListenerData>();

    private void Awake()
    {
        _timeRunner = new TimeRunner(this);
        Setup();
    }

    private void Setup()
    {
        SetupDependencies();
    }

    public void RegisterListener(IListener listener, float threshold)
    {
        _listenersData.Add(new ListenerData {Listener = listener, Threshold = threshold});
        listener.ID = _listenersData.Count - 1;
    }

    public void UnregisterListener(IListener listener)
    {
        _listenersData.RemoveAt(listener.ID);

        for (int i = 0; i < _listenersData.Count; i++)
        {
            _listenersData[i].Listener.ID = i;
        }
    }

    public void Move(Vector3[] points)
    {
        _points = points;
        _timeRunner.Begin(2f);
        foreach (var l in _listenersData)
        {
            l.Listener.OnBegin();
        }
    }

    private void Update()
    {
        _timeRunner.Update(Time.deltaTime);
    }

    public void HandleTimeRunnerValue(float t)
    {
        foreach (var l in _listenersData)
        {
            if (!l.Notified && t >= l.Threshold)
            {
                l.Listener.OnThresholdExceeded();
                l.Notified = true;
            }
        }

        var pos = Bezier.ComputeBezierCurve3D(_points, t);
        _transform.position = pos;
    }

    public void OnComplete()
    {
        foreach (var l in _listenersData)
        {
            l.Listener.OnEnd();
        }
    }

    private void SetupDependencies()
    {
        _transform = GetComponent<Transform>();
    }

    private Transform _transform;

    public interface IListener
    {
        void OnBegin();
        void OnThresholdExceeded();
        void OnEnd();
        int ID { get; set; }
    }

    private class ListenerData
    {
        public IListener Listener;
        public float Threshold;
        public bool Notified;
    }
}