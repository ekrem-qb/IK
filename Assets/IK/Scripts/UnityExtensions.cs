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
	public static void SortByDistanceTo<T>(this List<T> list, Vector3 target)
	{
		list.Sort(OrderByDistance);

		int OrderByDistance(T a, T b)
		{
			if (Vector3.Distance((a as MonoBehaviour).transform.position, target) < Vector3.Distance((b as MonoBehaviour).transform.position, target))
			{
				return -1;
			}

			if (Vector3.Distance((a as MonoBehaviour).transform.position, target) > Vector3.Distance((b as MonoBehaviour).transform.position, target))
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

	/// <summary>
	/// Extension method to remap a float number from one range to another
	/// </summary>
	/// <param name="value"></param>
	/// <param name="fromMin"></param>
	/// <param name="fromMax"></param>
	/// <param name="toMin"></param>
	/// <param name="toMax"></param>
	/// <returns></returns>
	public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
	{
		return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
	}

	/// <summary>
	/// Sets a joint's targetRotation to match a given local rotation.
	/// The joint transform's local rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
	{
		if (joint.configuredInWorldSpace)
		{
			Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
		}

		SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
	}

	/// <summary>
	/// Sets a joint's targetRotation to match a given world rotation.
	/// The joint transform's world rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
	{
		if (!joint.configuredInWorldSpace)
		{
			Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
		}

		SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
	}

	static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
	{
		// Calculate the rotation expressed by the joint's axis and secondary axis
		var right = joint.axis;
		var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
		var up = Vector3.Cross(forward, right).normalized;
		Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

		// Transform into world space
		Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

		// Counter-rotate and apply the new local rotation.
		// Joint space is the inverse of world space, so we need to invert our value
		if (space == Space.World)
		{
			resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
		}
		else
		{
			resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
		}

		// Transform back into joint space
		resultRotation *= worldToJointSpace;

		// Set target rotation to our newly calculated rotation
		joint.targetRotation = resultRotation;
	}
}