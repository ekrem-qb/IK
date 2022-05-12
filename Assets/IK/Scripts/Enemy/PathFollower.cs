using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
	public List<Transform> path = new List<Transform>();
	public bool loop = true;

	[Tooltip("Minimum waiting interval between points")]
	public float minInterval = 0.5f;

	[Tooltip("Maximum waiting interval between points")]
	public float maxInterval = 2;

	[Tooltip("Probability of Enemy stop and waiting in random point")] [Range(0, 100)]
	public int probabilityPercent = 50;

	[HideInInspector] public int nextPoint = 0;
	[ReadOnly] public bool isWaiting = false;
	private Enemy _enemy;

	private void Awake()
	{
		_enemy = this.GetComponent<Enemy>();
	}

	private void FixedUpdate()
	{
		if (!isWaiting && path.Count > 0)
		{
			Vector3 target = path[nextPoint].position;
			target.y = this.transform.position.y;

			if (Vector3.Distance(this.transform.position, target) > 0.5f)
			{
				_enemy.agent.SetDestination(target);
				Vector3 agentNextPosition = _enemy.agent.nextPosition;
				agentNextPosition.y = this.transform.position.y;
				_enemy.aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(agentNextPosition - _enemy.aprController.root.transform.position));
				_enemy.agent.speed = Vector3.Distance(this.transform.position, agentNextPosition).Remap(0, 2, _enemy.aprController.moveSpeed, 0);

				Vector3 direction = _enemy.aprController.root.transform.forward;
				direction.y = 0f;
				direction *= _enemy.aprController.moveSpeed / 4;

				_enemy.aprController.root.rigidbody.velocity = Vector3.Lerp(_enemy.aprController.root.rigidbody.velocity, direction + new Vector3(0, _enemy.aprController.root.rigidbody.velocity.y, 0), Time.fixedDeltaTime * 10);

				if (_enemy.aprController.isBalanced)
				{
					if (!_enemy.aprController.walkForward && !_enemy.aprController.moveAxisUsed)
					{
						_enemy.aprController.walkForward = true;
						_enemy.aprController.moveAxisUsed = true;
						_enemy.aprController.isKeyDown = true;
					}
				}
			}
			else
			{
				if (_enemy.aprController.walkForward && _enemy.aprController.moveAxisUsed)
				{
					_enemy.aprController.walkForward = false;
					_enemy.aprController.moveAxisUsed = false;
					_enemy.aprController.isKeyDown = false;
				}

				if (nextPoint < path.Count - 1)
				{
					nextPoint++;
					StartCoroutine(WaitForIntervalBetweenPoints());
				}
				else if (loop)
				{
					nextPoint = 0;
					StartCoroutine(WaitForIntervalBetweenPoints());
				}
			}
		}
		else
		{
			_enemy.aprController.walkForward = false;
			_enemy.aprController.moveAxisUsed = false;
			_enemy.aprController.isKeyDown = false;
		}
	}

	private void OnDrawGizmos()
	{
		// Drawing path gizmo

		Gizmos.color = Color.cyan;

		for (int i = 0; i < path.Count; i++)
		{
			if (path[i])
			{
				// Drawing points
				Gizmos.DrawSphere(path[i].position, 0.25f);
				if (i < path.Count - 1)
				{
					if (path[i + 1])
					{
						// Drawing lines between points
						Gizmos.DrawLine(path[i].position, path[i + 1].position);
					}
				}
			}
		}

		if (loop && path.Count > 2)
		{
			if (path[path.Count - 1] && path[0] && (path[path.Count - 1] != path[0]))
			{
				// Drawing connecting line between first and last point
				Gizmos.DrawLine(path[path.Count - 1].position, path[0].position);
			}
		}
	}

	private IEnumerator WaitForIntervalBetweenPoints()
	{
		if (Random.Range(0, 101) <= probabilityPercent)
		{
			isWaiting = true;
			yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
			isWaiting = false;
		}
	}
}