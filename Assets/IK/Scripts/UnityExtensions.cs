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

    /// <summary>
    /// Extension method to draw arrow
    /// </summary>
    /// <param name="gizmos"></param>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <param name="arrowHeadLength"></param>
    /// <param name="arrowHeadAngle"></param>
    /// <returns></returns>
    public static void DrawArrow(this Gizmos gizmos, Vector3 position, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(position, direction);

        Vector3 top = Quaternion.LookRotation(direction) * Quaternion.Euler(-90 + arrowHeadAngle, 0, 0) * new Vector3(0, 1, 0);
        Gizmos.DrawRay(position + direction, top * arrowHeadLength);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(position + direction, left * arrowHeadLength);
        Vector3 bottom = Quaternion.LookRotation(direction) * Quaternion.Euler(-90 - arrowHeadAngle, 0, 0) * new Vector3(0, 1, 0);
        Gizmos.DrawRay(position + direction, bottom * arrowHeadLength);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(position + direction, right * arrowHeadLength);
    }
}