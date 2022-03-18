using System.Collections;
using UnityEngine;

public class Gangster : Enemy
{
    WeaponManager weaponManager;
    public float attackInterval = 0.5f;
    bool isAttacking;

    void Start()
    {
        weaponManager = APR.COMP.GetComponent<WeaponManager>();
    }

    void FixedUpdate()
    {
        if (player)
        {
            Vector3 target = player.transform.position;
            target.y = this.transform.position.y;

            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - rootJoint.transform.position));

            if (Vector3.Distance(this.transform.position, target) > attackDistance)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;
                direction *= APR.moveSpeed;

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

                if (APR.balanced)
                {
                    if (!isAttacking)
                    {
                        StartCoroutine(Attack());
                    }
                }
            }
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
}