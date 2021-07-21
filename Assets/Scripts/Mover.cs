using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Mover
{
    private Queue<Vector3> queue = new Queue<Vector3>();
    public Queue<Vector3> Queue => queue;

    private Transform transform;

    public bool IsJumpingInQueue { get; private set; } = false;

    public Mover(Transform transform)
    {
        this.transform = transform;
    }

    public void JumpTo(Vector3 position, Action onComplete)
    {
        transform.DOJump(position, 1, 1, 0.2f).OnComplete(() => onComplete?.Invoke());
    }

    public void EnqueueTarget(Vector3 p) => queue.Enqueue(p);

    public void JumpInQueue()
    {
        if (queue.Count > 0)
        {
            IsJumpingInQueue = true;
            var p = queue.Dequeue();
            JumpTo(p, () => { JumpInQueue(); });
        }
        else
        {
            IsJumpingInQueue = false;
        }
    }
}