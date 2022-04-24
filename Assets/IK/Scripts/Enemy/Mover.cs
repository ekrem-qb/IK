using System.Collections;
using UnityEngine;

public class Mover : Enemy
{
	public float pickUpDelay = 0.065f;
	public Transform conveyorStart;
	public BoxManager boxManager;
	public Toggler toggler;
	public float attackInterval = 0.5f;
	Box _box;
	FixedJoint jointLeft, jointRight;
	SphereCollider trigger;
	WeaponManager weaponManager;

	Box box
	{
		get => _box;
		set
		{
			if (_box)
			{
				pathFollower.path.Remove(_box.transform);
			}

			if (value)
			{
				if (!pathFollower.path.Contains(value.transform))
				{
					pathFollower.path.Add(value.transform);
				}
			}

			_box = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		trigger = this.GetComponent<SphereCollider>();
		weaponManager = aprController.COMP.GetComponent<WeaponManager>();
		boxManager.boxes.CountChanged += OnBoxesCountChanged;
		toggler.Toggle += OnSwitchToggle;

		if (weaponManager.weapon)
		{
			weaponManager.weapon.gameObject.SetActive(false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		boxManager.boxes.CountChanged -= OnBoxesCountChanged;
		toggler.Toggle -= OnSwitchToggle;
	}

	void OnDrawGizmos()
	{
		if (pathFollower)
		{
			if (pathFollower.path.Count > 0)
			{
				if (pathFollower.path[0])
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(this.transform.position, pathFollower.path[0].position);
				}
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (pathFollower.enabled)
		{
			if (pathFollower.path.Count > 0)
			{
				if (pathFollower.path[0] == conveyorStart)
				{
					if (other.GetComponent<Conveyor>())
					{
						StartCoroutine(Drop());
					}
				}
				else if (box)
				{
					if (pathFollower.path[0] == box.transform)
					{
						if (other.transform == box.transform)
						{
							if (!jointLeft && !jointRight)
							{
								StartCoroutine(PickUp(other));
							}
						}
					}
				}
			}
		}
	}

	void OnSwitchToggle(bool isOn)
	{
		if (isOn)
		{
			if (player)
			{
				pathFollower.enabled = false;
				this.enabled = true;

				if (weaponManager.weapon)
				{
					weaponManager.weapon.gameObject.SetActive(true);
				}

				StartCoroutine(Drop());
			}
		}
		else
		{
			pathFollower.enabled = true;
			this.enabled = false;

			if (weaponManager.weapon)
			{
				weaponManager.weapon.gameObject.SetActive(false);
			}
		}
	}

	void OnBoxesCountChanged(int count)
	{
		if (count > 0)
		{
			boxManager.boxes.SortByDistanceTo(this.transform.position);
			box = boxManager.boxes[0];
		}
		else
		{
			box = null;
		}
	}

	IEnumerator PickUp(Collider coll)
	{
		pathFollower.isWaiting = true;
		box = null;

		Vector3 targetPos = coll.transform.position;
		targetPos.y = this.transform.position.y;
		aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(targetPos - this.transform.position));
		aprController.body.joint.targetRotation = new Quaternion(aprController.MouseYAxisBody, 0, 0, 1);

		yield return new WaitForSeconds(pickUpDelay * 5);

		aprController.isGrabbing = true;

		//upper  left arm pose
		aprController.armLeft.joint.targetRotation = new Quaternion(-0.88f - aprController.MouseYAxisArms, 0.58f + aprController.MouseYAxisArms, -0.8f, 1);

		//upper  left arm pose
		aprController.armRight.joint.targetRotation = Quaternion.Inverse(new Quaternion(0.88f - aprController.MouseYAxisArms, 0.58f + aprController.MouseYAxisArms, -0.8f, 1));

		yield return new WaitForSeconds(pickUpDelay);

		jointLeft = aprController.handLeft.transform.gameObject.AddComponent<FixedJoint>();
		jointLeft.breakForce = Mathf.Infinity;
		jointLeft.connectedBody = coll.attachedRigidbody;

		jointRight = aprController.handRight.transform.gameObject.AddComponent<FixedJoint>();
		jointRight.breakForce = Mathf.Infinity;
		jointRight.connectedBody = coll.attachedRigidbody;

		aprController.body.joint.targetRotation = new Quaternion(0, 0, 0, 1);

		yield return new WaitForSeconds(pickUpDelay * 5);

		if (!pathFollower.path.Contains(conveyorStart))
		{
			pathFollower.path.Insert(0, conveyorStart);
		}

		pathFollower.isWaiting = false;
		trigger.radius = 1.5f;
	}

	public IEnumerator Drop()
	{
		pathFollower.isWaiting = true;

		Vector3 targetPos = conveyorStart.position;
		targetPos.y = this.transform.position.y;
		aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(targetPos - this.transform.position));

		yield return new WaitForSeconds(pickUpDelay * 5);

		aprController.isGrabbing = false;

		yield return new WaitForSeconds(pickUpDelay);

		Destroy(jointLeft);
		Destroy(jointRight);

		pathFollower.isWaiting = false;
		pathFollower.path.Remove(conveyorStart);

		trigger.radius = 1;
	}

	protected override void OnPlayerChanged(Player newPlayer)
	{
		if (newPlayer)
		{
			if (toggler.isOn)
			{
				pathFollower.enabled = false;
				this.enabled = true;

				if (weaponManager.weapon)
				{
					weaponManager.weapon.gameObject.SetActive(true);
				}

				StartCoroutine(Drop());
			}
		}
		else
		{
			pathFollower.enabled = true;
			this.enabled = false;

			if (weaponManager.weapon)
			{
				weaponManager.weapon.gameObject.SetActive(false);
			}
		}
	}

	protected override IEnumerator Attack()
	{
		isAttacking = true;

		if (weaponManager.weapon)
		{
			if (weaponManager.weapon is Thrower)
			{
				Thrower thrower = weaponManager.weapon as Thrower;
				thrower.Attack(player.transform.position);
				yield return new WaitUntil(() => thrower.isAttacking);
				thrower.meshRenderer.enabled = false;
				yield return new WaitForSeconds(attackInterval * 4);
				thrower.meshRenderer.enabled = true;
			}
		}

		yield return new WaitForSeconds(attackInterval);
		isAttacking = false;
	}
}