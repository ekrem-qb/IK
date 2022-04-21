using System;
using System.Collections;
using UnityEngine;

public class Melee : Weapon
{
	[Header("Melee")] public float force = 50;

	[HideInInspector] public bool isAttacking;
	private bool _isInHook;
	protected Action<bool> isInHookChanged = newIsInHook => { };

	protected bool isInHook
	{
		get => _isInHook;
		set
		{
			if (_isInHook != value)
			{
				isInHookChanged(value);
			}

			_isInHook = value;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isAttacking)
		{
			if (isPickedUp)
			{
				if (!other.isTrigger)
				{
					if (this.transform.root != other.transform.root)
					{
						HealthManager enemy = other.transform.root.GetComponent<HealthManager>();
						if (enemy)
						{
							enemy.health -= damage;
							Rigidbody rb = other.GetComponent<Rigidbody>();
							if (rb)
							{
								rb.AddForce(this.transform.right * force, ForceMode.Impulse);
							}
						}

						if (particle)
						{
							particle.Play();
						}
					}
				}
			}
		}
	}

	public override void Attack()
	{
		if (!isInHook)
		{
			StartCoroutine(Hook());
		}
	}

	protected IEnumerator Hook()
	{
		isInHook = true;

		arm.joint.angularXDrive = aprController.ReachStiffness;
		arm.joint.angularYZDrive = aprController.ReachStiffness;
		armLow.joint.angularXDrive = aprController.ReachStiffness;
		armLow.joint.angularYZDrive = aprController.ReachStiffness;

		arm.joint.targetRotation = new Quaternion(-0.62f, 0.51f, 0.02f, 1);
		armLow.joint.targetRotation = new Quaternion(1.31f, -0.5f, -0.5f, 1);

		yield return new WaitForSeconds(0.25f);

		arm.joint.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
		armLow.joint.targetRotation = new Quaternion(0.2f, 0, 0, 1);

		if (!(this is Fist))
		{
			hand.joint.targetRotation = new Quaternion(-0.75f, -0.04f, -0.1f, 1);
		}

		isAttacking = true;

		yield return new WaitForSeconds(0.25f);

		arm.joint.targetRotation = arm.originalRotation;
		armLow.joint.targetRotation = armLow.originalRotation;
		hand.joint.targetRotation = hand.originalRotation;

		arm.joint.angularXDrive = aprController.DriveOff;
		arm.joint.angularYZDrive = aprController.DriveOff;
		armLow.joint.angularXDrive = aprController.DriveOff;
		armLow.joint.angularYZDrive = aprController.DriveOff;

		isAttacking = false;
		isInHook = false;
	}
}