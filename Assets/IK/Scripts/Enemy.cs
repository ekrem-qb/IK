using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector]
    public ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;
    Rigidbody rootRB;
    [HideInInspector]
    public Player player;
    WeaponManager weaponManager;
    public float attackDistance = 2;
    public float attackInterval = 0.5f;
    bool isAttacking;

    public List<Transform> path;
    public bool loop = true;
    int nextPoint = 0;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        rootRB = APR.Root.GetComponent<Rigidbody>();
        weaponManager = APR.COMP.GetComponent<WeaponManager>();
    }

    void FixedUpdate()
    {
        if (player || (path.Count > 0 && path[nextPoint]))
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

            if (Vector3.Distance(this.transform.position, target) > attackDistance)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;
                rootRB.velocity = Vector3.Lerp(rootRB.velocity, (direction * APR.moveSpeed) + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

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
                }
                else if (loop)
                {
                    nextPoint = 0;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance);
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

    void OnDestroy()
    {
        if (player)
        {
            player.nearEnemies.Remove(this);
        }
    }
}