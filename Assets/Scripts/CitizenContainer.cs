using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CitizenContainer : MonoBehaviour
{
    public List<Citizen> Bunnies { get; private set; } = new List<Citizen>();

    public event Action<CitizenContainer> OnEmpty = delegate { };

    public void Grasp(Citizen citizen)
    {
        Bunnies.Add(citizen);
    }

    public void Grasp(CitizenContainer other, bool localize = true)
    {
        foreach (var b in other.Bunnies)
        {
            Grasp(b);
            if (localize)
            {
                Localize(b.transform);
            }
        }

        other.Bunnies.Clear();
        other.OnEmpty?.Invoke(other);
    }

    public void Grasp(List<Citizen> otherBunnies, bool localize = true)
    {
        foreach (var b in otherBunnies)
        {
            Grasp(b);
            if (localize)
            {
                Localize(b.transform);
            }
        }

        otherBunnies.Clear();
    }

    public void Localize(Transform t)
    {
        t.SetParent(transform);
        t.localPosition = SpawnRandomPosition();
    }

    public Vector3 SpawnRandomPosition(bool local = true)
    {
        var randomPos = Random.insideUnitCircle * 0.2f;
        var offset = transform.up * 0.5f + new Vector3(randomPos.x, 0f, randomPos.y);
        var pos = Vector3.Scale(offset, transform.localScale);
        if (!local)
        {
            pos = transform.TransformPoint(pos);
        }

        return pos;
    }
}