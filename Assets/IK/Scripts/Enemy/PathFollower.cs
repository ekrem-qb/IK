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
    int nextPoint = 0;
    [HideInInspector] public bool isWaiting = false;
    Enemy enemy;

    void Awake()
    {
        enemy = this.GetComponent<Enemy>();
    }

    void FixedUpdate()
    {
        if (!isWaiting && !enemy.player)
        {
            Vector3 target = path[nextPoint].position;
            target.y = this.transform.position.y;

            enemy.rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - enemy.rootJoint.transform.position));

            if (Vector3.Distance(this.transform.position, target) > 0.1f)
            {
                Vector3 direction = enemy.APR.Root.transform.forward;
                direction.y = 0f;
                direction *= enemy.APR.moveSpeed / 4;

                enemy.rootRB.velocity = Vector3.Lerp(enemy.rootRB.velocity, direction + new Vector3(0, enemy.rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

                if (enemy.APR.balanced)
                {
                    if (!enemy.APR.WalkForward && !enemy.APR.moveAxisUsed)
                    {
                        enemy.APR.WalkForward = true;
                        enemy.APR.moveAxisUsed = true;
                        enemy.APR.isKeyDown = true;
                    }
                }
            }
            else
            {
                if (enemy.APR.WalkForward && enemy.APR.moveAxisUsed)
                {
                    enemy.APR.WalkForward = false;
                    enemy.APR.moveAxisUsed = false;
                    enemy.APR.isKeyDown = false;
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
        else if (enemy.APR.WalkForward && enemy.APR.moveAxisUsed)
        {
            enemy.APR.WalkForward = false;
            enemy.APR.moveAxisUsed = false;
            enemy.APR.isKeyDown = false;
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
}