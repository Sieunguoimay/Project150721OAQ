using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenContainer : MonoBehaviour
{
    public List<Bunnie> Bunnies { get; private set; } = new List<Bunnie>();

    public void Grasp(Bunnie bunnie)
    {
        Bunnies.Add(bunnie);
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
    }

    public void Grasp(List<Bunnie> otherBunnies, bool localize = true)
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
        var randomPos = Random.insideUnitCircle * 0.5f;
        var offset = transform.up * 0.5f + new Vector3(randomPos.x, t.localScale.y, randomPos.y);
        t.SetParent(transform);
        t.localPosition = Vector3.Scale(offset, transform.localScale);
    }
}