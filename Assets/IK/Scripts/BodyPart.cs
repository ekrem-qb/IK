using UnityEngine;

public struct BodyPart
{
	public Transform transform;
	public ConfigurableJoint joint;
	public Rigidbody rigidbody;
	public Quaternion originalRotation;
}