using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : Target
{
	[HideInInspector] public APRController aprController;
	[Header("Enemy")] public float attackDistance = 2.5f;
	[ReadOnly] [SerializeField] protected bool isAttacking;
	public NavMeshAgent agent;
	protected PathFollower pathFollower;

	protected override void Awake()
	{
		aprController = this.transform.root.GetComponent<APRController>();
		if (!aprController.root.transform)
		{
			aprController.PlayerSetup();
		}

		agent.updatePosition = false;
		agent.updateRotation = false;

		selfTarget = aprController.body.transform;
		pathFollower = this.GetComponent<PathFollower>();
		this.enabled = false;
	}

	protected virtual void FixedUpdate()
	{
		// Following Player

		Vector3 target = player.transform.position;
		target.y = this.transform.position.y;

		agent.SetDestination(target);
		Vector3 agentNextPosition = agent.nextPosition;
		agentNextPosition.y = this.transform.position.y;
		aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(agentNextPosition - aprController.root.transform.position));
		agent.speed = Vector3.Distance(this.transform.position, agentNextPosition).Remap(0, 2, aprController.moveSpeed, 0);

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
			aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - aprController.root.transform.position));

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

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(this.transform.position, attackDistance);
	}

	protected abstract IEnumerator Attack();

	protected override void OnPlayerChanged(Player newPlayer)
	{
		// Disabling following Player if it goes far away and enabling if it is near 
		base.OnPlayerChanged(newPlayer);
		// Disabling following path if Player is near and enabling if it goes far away 
		pathFollower.enabled = !newPlayer;
	}
}