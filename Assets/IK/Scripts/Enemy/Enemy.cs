using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
	[HideInInspector] public APRController aprController;
	[HideInInspector] public Transform selfTarget;
	[SerializeField] [ReadOnly] private Player _player;
	public float attackDistance = 2.5f;
	protected bool isAttacking;
	protected PathFollower pathFollower;

	public Player player
	{
		get => _player;
		set
		{
			if (_player != value)
			{
				OnPlayerChanged(value);
			}

			_player = value;
		}
	}

	protected virtual void Awake()
	{
		aprController = this.transform.root.GetComponent<APRController>();
		selfTarget = aprController.body.transform;
		pathFollower = this.GetComponent<PathFollower>();
		this.enabled = false;
	}

	protected virtual void FixedUpdate()
	{
		Vector3 target = player.transform.position;
		target.y = this.transform.position.y;

		aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - aprController.root.transform.position));

		if (Vector3.Distance(this.transform.position, target) > attackDistance)
		{
			Vector3 direction = aprController.root.transform.forward;
			direction.y = 0f;
			direction *= aprController.moveSpeed;

			aprController.root.rigidbody.velocity = Vector3.Lerp(aprController.root.rigidbody.velocity, direction + new Vector3(0, aprController.root.rigidbody.velocity.y, 0), Time.fixedDeltaTime * 10);

			if (aprController.isBalanced)
			{
				if (!aprController.walkForward && !aprController.moveAxisUsed)
				{
					aprController.walkForward = true;
					aprController.moveAxisUsed = true;
					aprController.isKeyDown = true;
				}
			}
		}
		else
		{
			if (aprController.walkForward && aprController.moveAxisUsed)
			{
				aprController.walkForward = false;
				aprController.moveAxisUsed = false;
				aprController.isKeyDown = false;
			}

			if (aprController.isBalanced)
			{
				if (!isAttacking)
				{
					StartCoroutine(Attack());
				}
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (aprController.walkForward && aprController.moveAxisUsed)
		{
			aprController.walkForward = false;
			aprController.moveAxisUsed = false;
			aprController.isKeyDown = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (player)
		{
			player.nearEnemies.Remove(this);
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(this.transform.position, attackDistance);
	}

	protected abstract IEnumerator Attack();

	protected virtual void OnPlayerChanged(Player newPlayer)
	{
		pathFollower.enabled = !newPlayer;
		this.enabled = newPlayer;
	}
}