using ARP.APR.Scripts;
using UnityEngine;

public class RotationControl : MonoBehaviour
{
	private APRController _aprController;
	private Vector3 _targetDirection = Vector3.zero;

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		if (!_aprController.root.transform)
		{
			_aprController.PlayerSetup();
		}
	}

	private void FixedUpdate()
	{
		if (_aprController.useControls)
		{
			if (Input.GetAxisRaw(_aprController.leftRight) != 0)
			{
				_targetDirection += Vector3.left * Input.GetAxisRaw(_aprController.leftRight);
			}

			if (Input.GetAxisRaw(_aprController.forwardBackward) != 0)
			{
				_targetDirection += Vector3.forward * Input.GetAxisRaw(_aprController.forwardBackward);
			}

			if (_aprController.joystick)
			{
				if (_aprController.joystick.Horizontal != 0)
				{
					_targetDirection += Vector3.left * _aprController.joystick.Horizontal;
				}

				if (_aprController.joystick.Vertical != 0)
				{
					_targetDirection += Vector3.forward * _aprController.joystick.Vertical;
				}
			}

			if (_targetDirection != Vector3.zero)
			{
				_aprController.root.joint.targetRotation = Quaternion.LookRotation(_targetDirection);

				_targetDirection = Vector3.zero;
			}
		}
	}
}