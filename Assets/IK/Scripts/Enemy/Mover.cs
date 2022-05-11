using System.Collections;
using UnityEngine;

public class Mover : Ped
{
	[Header("Mover")] public float pickUpDelay = 0.065f;

	public Transform conveyorStart;
	public BoxManager boxManager;
	public Toggler toggler;
	private Box _box;
	private FixedJoint _jointLeft, _jointRight;
	private SphereCollider _trigger;

	private Box box
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
		_trigger = this.GetComponent<SphereCollider>();
		boxManager.boxes.CountChanged += OnBoxesCountChanged;

		if(toggler)
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
		if (toggler)
			toggler.Toggle -= OnSwitchToggle;
	}

	private void OnDrawGizmos()
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

	private void OnTriggerEnter(Collider other)
	{
		if (pathFollower.enabled)
		{
			if (pathFollower.path.Count > 0)
			{
				if (pathFollower.path[0] == conveyorStart)
				{
					if (other.GetComponent<Conveyor>())
					{
						if (other.transform.parent == conveyorStart.parent)
						{
							StartCoroutine(Drop());
						}
					}
				}
				else if (box)
				{
					if (pathFollower.path[0] == box.transform)
					{
						if (other.transform == box.transform)
						{
							if (!_jointLeft && !_jointRight)
							{
								StartCoroutine(PickUp(other));
							}
						}
					}
				}
			}
		}
	}

	private void OnSwitchToggle(bool isOn)
	{
		if (isOn)
		{
			Annoy();
		}
		else
		{
			Calm();
		}
	}

	private void OnBoxesCountChanged(int count)
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

	private IEnumerator PickUp(Collider coll)
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
		aprController.armLeft.joint.targetRotation = new Quaternion(-0.88f, 0.58f, -0.8f, 1);

		//upper  left arm pose
		aprController.armRight.joint.targetRotation = Quaternion.Inverse(new Quaternion(0.88f, 0.58f, -0.8f, 1));

		yield return new WaitForSeconds(pickUpDelay);

		_jointLeft = aprController.handLeft.transform.gameObject.AddComponent<FixedJoint>();
		_jointLeft.breakForce = Mathf.Infinity;
		_jointLeft.connectedBody = coll.attachedRigidbody;

		_jointRight = aprController.handRight.transform.gameObject.AddComponent<FixedJoint>();
		_jointRight.breakForce = Mathf.Infinity;
		_jointRight.connectedBody = coll.attachedRigidbody;

		aprController.body.joint.targetRotation = new Quaternion(0, 0, 0, 1);

		yield return new WaitForSeconds(pickUpDelay * 5);

		if (!pathFollower.path.Contains(conveyorStart))
		{
			pathFollower.path.Insert(0, conveyorStart);
		}

		pathFollower.isWaiting = false;
		_trigger.radius = 1.5f;
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

		Destroy(_jointLeft);
		Destroy(_jointRight);

		pathFollower.isWaiting = false;
		pathFollower.path.Remove(conveyorStart);

		_trigger.radius = 1;
	}

	protected override void OnPlayerChanged(Player newPlayer)
	{
		if (newPlayer)
		{
			if (toggler.isOn)
			{
				Annoy();
			}
		}
		else
		{
			Calm();
		}
	}

	public override void Calm()
	{
		base.Calm();

		if (weaponManager.weapon)
		{
			weaponManager.weapon.gameObject.SetActive(false);
		}
	}

	public override void Annoy()
	{
		base.Annoy();

		if (player)
		{
			if (weaponManager.weapon)
			{
				weaponManager.weapon.gameObject.SetActive(true);
			}

			StartCoroutine(Drop());
		}
	}

	protected override IEnumerator Attack()
	{
		isAttacking = true;

		if (weaponManager.weapon)
		{
			if (weaponManager.weapon is Thrower thrower)
			{
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