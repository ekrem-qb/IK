using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
	public List<Transform> path = new List<Transform>();
	public bool loop = true;
	public float minInterval = 0.5f;
	public float maxInterval = 2;
	[Range(0, 100)] public int probabilityPercent = 50;
	[HideInInspector] public int nextPoint = 0;
	public bool isWaiting = false;
	Enemy enemy;

	void Awake()
	{
		enemy = this.GetComponent<Enemy>();
	}

	void FixedUpdate()
	{
		if (!isWaiting && path.Count > 0)
		{
			Vector3 target = path[nextPoint].position;
			target.y = this.transform.position.y;

			if (Vector3.Distance(this.transform.position, target) > 0.5f)
			{
				enemy.aprController.root.joint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - enemy.aprController.root.transform.position));

				Vector3 direction = enemy.aprController.root.transform.forward;
				direction.y = 0f;
				direction *= enemy.aprController.moveSpeed / 4;

				enemy.aprController.root.rigidbody.velocity = Vector3.Lerp(enemy.aprController.root.rigidbody.velocity, direction + new Vector3(0, enemy.aprController.root.rigidbody.velocity.y, 0), Time.fixedDeltaTime * 10);

				if (enemy.aprController.isBalanced)
				{
					if (!enemy.aprController.walkForward && !enemy.aprController.moveAxisUsed)
					{
						enemy.aprController.walkForward = true;
						enemy.aprController.moveAxisUsed = true;
						enemy.aprController.isKeyDown = true;
					}
				}
			}
			else
			{
				if (enemy.aprController.walkForward && enemy.aprController.moveAxisUsed)
				{
					enemy.aprController.walkForward = false;
					enemy.aprController.moveAxisUsed = false;
					enemy.aprController.isKeyDown = false;
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
			enemy.aprController.walkForward = false;
			enemy.aprController.moveAxisUsed = false;
			enemy.aprController.isKeyDown = false;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;

		for (int i = 0; i < path.Count; i++)
		{
			if (path[i])
			{
				Gizmos.DrawSphere(path[i].position, 0.25f);
				if (i < path.Count - 1)
				{
					if (path[i + 1])
					{
						Gizmos.DrawLine(path[i].position, path[i + 1].position);
					}
				}
			}
		}

		if (loop && path.Count > 2)
		{
			if (path[path.Count - 1] && path[0] && (path[path.Count - 1] != path[0]))
			{
				Gizmos.DrawLine(path[path.Count - 1].position, path[0].position);
			}
		}
	}

	IEnumerator WaitForIntervalBetweenPoints()
	{
		if (Random.Range(0, 101) <= probabilityPercent)
		{
			isWaiting = true;
			yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
			isWaiting = false;
		}
	}
}