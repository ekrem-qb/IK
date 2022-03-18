using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    /// <summary>
    /// Extension method to get nearest to target item
    /// </summary>
    /// <param name="list"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static void SortByDistanceTo(this IList list, Vector3 target)
    {
        (list as List<MonoBehaviour>)?.Sort(OrderByDistance);

        int OrderByDistance(MonoBehaviour a, MonoBehaviour b)
        {
            if (Vector3.Distance(a.transform.position, target) < Vector3.Distance(b.transform.position, target))
            {
                return -1;
            }
            else if (Vector3.Distance(a.transform.position, target) > Vector3.Distance(b.transform.position, target))
            {
                return 1;
            }

            return 0;
        }
    }
}