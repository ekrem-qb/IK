using ARP.APR.Scripts;
using UnityEngine;

public class RotationControl : MonoBehaviour
{
	private APRController _aprController;
	private ConfigurableJoint _rootJoint;

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		_rootJoint = _aprController.Root.GetComponent<ConfigurableJoint>();
	}

	private void FixedUpdate()
	{
		if (_aprController.useControls)
		{
			Vector3 targetDirection = Vector3.zero;

			if (Input.GetAxisRaw(_aprController.leftRight) != 0)
			{
				targetDirection += Vector3.left * Input.GetAxisRaw(_aprController.leftRight);
			}

			if (Input.GetAxisRaw(_aprController.forwardBackward) != 0)
			{
				targetDirection += Vector3.forward * Input.GetAxisRaw(_aprController.forwardBackward);
			}

			if (_aprController.joystick)
			{
				if (_aprController.joystick.Horizontal != 0)
				{
					targetDirection += Vector3.left * _aprController.joystick.Horizontal;
				}

				if (_aprController.joystick.Vertical != 0)
				{
					targetDirection += Vector3.forward * _aprController.joystick.Vertical;
				}
			}

			if (targetDirection != Vector3.zero)
			{
				_rootJoint.targetRotation = Quaternion.LookRotation(targetDirection);
			}
		}
	}
}