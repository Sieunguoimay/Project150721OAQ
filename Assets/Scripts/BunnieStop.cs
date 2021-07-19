using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnieStop : MonoBehaviour
{
    public List<Bunnie> Bunnies { get; private set; } = new List<Bunnie>();

    public void Keep(Bunnie bunnie)
    {
        Bunnies.Add(bunnie);
        bunnie.transform.SetParent(transform);
        Vector2 randomPos = Random.insideUnitCircle * 0.5f;
        bunnie.transform.localPosition = Vector3.Scale(transform.up * 0.5f +
                                                       new Vector3(randomPos.x, bunnie.transform.localScale.y,
                                                           randomPos.y), transform.localScale);
    }

    public void TransferTo(ref List<Bunnie> target)
    {
        foreach (var b in Bunnies)
        {
            target.Add(b);
        }

        Bunnies.Clear();
    }

}