using System;
using System.Collections.Generic;

public static class CollectionUtility
{
    public static TItem GetRandom<TItem>(this IReadOnlyList<TItem> arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Count)];
    }
}