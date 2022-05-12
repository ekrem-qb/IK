using UnityEngine;

public class Slidable : MonoBehaviour
{
	private const float mass = 10000;
	public Toggler toggler;
	public ConfigurableJoint movingPart;
	public Vector3 movingPartToggleOffset;
	public float transitionSpeed = 15;

	private void Awake()
	{
		toggler.Toggle += OnSwitchToggle;
		Rigidbody rb = movingPart.GetComponent<Rigidbody>();
		// Rigidbody setup
		rb.mass = mass;
		rb.drag = mass;
		rb.useGravity = false;
		// Joint setup
		JointDrive drive = new JointDrive
		{
			positionSpring = mass * 100 * transitionSpeed,
			positionDamper = mass,
			maximumForce = mass * 100 * transitionSpeed,
		};
		if (movingPartToggleOffset.x != 0)
		{
			movingPart.xMotion = ConfigurableJointMotion.Limited;
			movingPart.xDrive = drive;
		}
		else
		{
			movingPart.xMotion = ConfigurableJointMotion.Locked;
		}

		if (movingPartToggleOffset.y != 0)
		{
			movingPart.yMotion = ConfigurableJointMotion.Limited;
			movingPart.yDrive = drive;
		}
		else
		{
			movingPart.yMotion = ConfigurableJointMotion.Locked;
		}

		if (movingPartToggleOffset.z != 0)
		{
			movingPart.zMotion = ConfigurableJointMotion.Limited;
			movingPart.zDrive = drive;
		}
		else
		{
			movingPart.zMotion = ConfigurableJointMotion.Locked;
		}

		movingPart.angularXMotion = ConfigurableJointMotion.Locked;
		movingPart.angularYMotion = ConfigurableJointMotion.Locked;
		movingPart.angularZMotion = ConfigurableJointMotion.Locked;
		SoftJointLimit limit = new SoftJointLimit
		{
			limit = Mathf.Max(Mathf.Abs(movingPartToggleOffset.x), Mathf.Abs(movingPartToggleOffset.y), Mathf.Abs(movingPartToggleOffset.z))
		};
		movingPart.linearLimit = limit;
	}

	private void OnDestroy()
	{
		toggler.Toggle -= OnSwitchToggle;
	}

	private void OnSwitchToggle(bool isOn)
	{
		if (isOn)
		{
			movingPart.targetPosition -= movingPartToggleOffset;
		}
		else
		{
			movingPart.targetPosition = Vector3.zero;
		}
	}
}