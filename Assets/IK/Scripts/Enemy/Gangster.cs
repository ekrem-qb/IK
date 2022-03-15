using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gangster : Enemy
{
    WeaponManager weaponManager;

    public float attackInterval = 0.5f;
    bool isAttacking;

    public List<Transform> path;
    public bool loop = true;
    public float minInterval = 0.5f;
    public float maxInterval = 2;
    [Range(0, 100)] public int probabilityPercent = 50;
    int nextPoint = 0;
    bool isWaiting = false;

    void Start()
    {
        weaponManager = APR.COMP.GetComponent<WeaponManager>();
    }

    void FixedUpdate()
    {
        if (!isWaiting && (player || (path.Count > 0 && path[nextPoint])))
        {
            Vector3 target = Vector3.zero;

            if (player)
            {
                target = player.transform.position;
            }
            else if (path[nextPoint])
            {
                target = path[nextPoint].position;
            }

            target.y = this.transform.position.y;

            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - rootJoint.transform.position));

            float distanceToStop = 0.1f;
            if (player)
            {
                distanceToStop = attackDistance;
            }

            if (Vector3.Distance(this.transform.position, target) > distanceToStop)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;

                if (player)
                {
                    direction *= APR.moveSpeed;
                }
                else
                {
                    direction *= APR.moveSpeed / 4;
                }

                rootRB.velocity = Vector3.Lerp(rootRB.velocity, direction + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

                if (APR.balanced)
                {
                    if (!APR.WalkForward && !APR.moveAxisUsed)
                    {
                        APR.WalkForward = true;
                        APR.moveAxisUsed = true;
                        APR.isKeyDown = true;
                    }
                }
            }
            else
            {
                if (APR.WalkForward && APR.moveAxisUsed)
                {
                    APR.WalkForward = false;
                    APR.moveAxisUsed = false;
                    APR.isKeyDown = false;
                }

                if (player)
                {
                    if (APR.balanced)
                    {
                        if (!isAttacking)
                        {
                            StartCoroutine(Attack());
                        }
                    }
                }
                else if (nextPoint < path.Count - 1)
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
        else if (APR.WalkForward && APR.moveAxisUsed)
        {
            APR.WalkForward = false;
            APR.moveAxisUsed = false;
            APR.isKeyDown = false;
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

    IEnumerator Attack()
    {
        isAttacking = true;

        if (weaponManager.weaponLeft)
        {
            weaponManager.weaponLeft.Attack();
        }

        if (weaponManager.weaponRight)
        {
            weaponManager.weaponRight.Attack();
        }

        yield return new WaitForSeconds(attackInterval);
        isAttacking = false;
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