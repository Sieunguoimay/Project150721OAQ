using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CitizenContainer : MonoBehaviour
{
    public List<Citizen> Citizens { get; private set; } = new List<Citizen>();

    public event Action<CitizenContainer> OnEmpty = delegate { };

    public void Grasp(Citizen citizen)
    {
        Citizens.Add(citizen);
    }

    public void Grasp(CitizenContainer other, bool localize = true)
    {
        foreach (var b in other.Citizens)
        {
            Grasp(b);
            if (localize)
            {
                Reposition(b.transform);
            }
        }

        other.Citizens.Clear();
        other.OnEmpty?.Invoke(other);
    }

    public void Grasp(List<Citizen> otherBunnies, int count = -1)
    {
        if (count == otherBunnies.Count || count == -1)
        {
            foreach (var b in otherBunnies)
            {
                Grasp(b);
            }

            otherBunnies.Clear();
        }
        else
        {
            int n = Mathf.Min(count, otherBunnies.Count);
            for (int i = n - 1; i >= 0; i--)
            {
                Grasp(otherBunnies[i]);
                otherBunnies.RemoveAt(i);
            }
        }
    }

    public void Reposition(Transform t)
    {
        t.position = SpawnRandomPosition(false);
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